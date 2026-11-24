using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public partial class MemberRepository
    {
        private const string MergeItemPrefix = IDPREFIX + ":Merge:";
        private const string MergeEntryAttribute = "MergeEntry";
        private const string MergeRevisionAttribute = "MergeRevision";
        private const string MergeSeasonsWrittenAttribute = "MergeSeasonsWritten";
        private const string AppliedMergeAttribute = "AppliedMemberMerge";

        public async Task<MemberMergePreview> PreviewMerge(MemberMergeRequest request)
        {
            ValidateMergeRequest(request);
            var register = await ReadRegister();
            var ledger = GetLedger(request, register);
            if (ledger.Revision != null)
            {
                throw new AppConflictException("Finish the pending source correction for this number first.");
            }
            return CreatePreview(request, register, ledger);
        }

        public async Task ApplyMerge(MemberMergeRequest request)
        {
            ValidateMergeRequest(request);
            if (string.IsNullOrWhiteSpace(request.PreviewToken))
            {
                throw new AppValidationException("Preview the selected records before applying the merge.");
            }

            var register = await ReadRegister();
            var ledger = GetLedger(request, register);
            if (WasApplied(ledger, request))
            {
                await FinalizeMerge(new FinalizeMemberMergeRequest(request.MembershipNumber, ledger.Revision));
                return;
            }

            if (ledger.Revision != null)
            {
                throw new AppConflictException("Finish the pending source correction for this number first.");
            }

            var preview = CreatePreview(request, register, ledger);
            if (preview.PreviewToken != request.PreviewToken)
            {
                throw new AppConflictException("The member records have changed. Load them again and preview the merge.");
            }

            var entry = new MemberMergeEntry(request.KeepDbKey, request.RetireDbKey,
                preview.Retire.SeasonsActive, preview.PreviewToken);
            var revision = Guid.NewGuid().ToString();
            var write = new PutAttributesRequest
            {
                DomainName = Domain,
                ItemName = LedgerKey(request.MembershipNumber),
                Expected = new UpdateCondition { Name = MergeRevisionAttribute, Exists = false },
                Attributes = new List<ReplaceableAttribute>
                {
                    // Keep the ledger in the same register query as its member records.
                    new ReplaceableAttribute { Name = "Name", Value = "Member merge history", Replace = true },
                    new ReplaceableAttribute { Name = MergeRevisionAttribute, Value = revision, Replace = true },
                    new ReplaceableAttribute { Name = MergeEntryAttribute, Value = JsonSerializer.Serialize(entry) }
                }
            };

            using var client = GetClient();
            try
            {
                // Keep season lookups valid if the process stops between the two member writes.
                await client.PutAttributesAsync(write);
            }
            catch (AmazonSimpleDBException ex) when (ex.ErrorCode is "ConditionalCheckFailed" or "AttributeDoesNotExist")
            {
                var current = GetLedger(request, await ReadRegister());
                if (!WasApplied(current, request))
                {
                    throw new AppConflictException("Another merge changed this membership number. Load the records and preview again.", ex);
                }
                revision = current.Revision;
            }

            await FinalizeMerge(new FinalizeMemberMergeRequest(request.MembershipNumber, revision));
        }

        public async Task<PendingMemberMerge[]> GetPendingMerges()
        {
            var register = await ReadRegister();
            return register.Ledgers.Select(pair => DescribePendingMerge(
                    int.Parse(pair.Key[MergeItemPrefix.Length..], CultureInfo.InvariantCulture), pair.Value, register))
                .OrderBy(merge => merge.MembershipNumber).ToArray();
        }

        public async Task FinalizeMerge(FinalizeMemberMergeRequest request)
        {
            if (request == null || request.MembershipNumber <= 0 || string.IsNullOrWhiteSpace(request.Revision))
            {
                throw new AppValidationException("Select a pending source correction.");
            }

            var register = await ReadRegister();
            var ledgerKey = LedgerKey(request.MembershipNumber);
            if (!register.Ledgers.TryGetValue(ledgerKey, out var ledger))
            {
                return;
            }
            if (ledger.Revision != request.Revision)
            {
                throw new AppConflictException("This pending correction has changed. Refresh the list before continuing.");
            }

            var pending = DescribePendingMerge(request.MembershipNumber, ledger, register);
            var number = request.MembershipNumber.ToString(CultureInfo.InvariantCulture);
            var keepKeys = ledger.Entries.Select(entry => entry.KeepDbKey).Except(pending.RetireDbKeys).ToArray();
            if (pending.Keep.Length != keepKeys.Length || register.Ledgers.Any(pair => pair.Key != ledgerKey &&
                pair.Value.Entries.Any(entry => keepKeys.Contains(entry.KeepDbKey) || pending.RetireDbKeys.Contains(entry.RetireDbKey) ||
                    keepKeys.Contains(entry.RetireDbKey) || pending.RetireDbKeys.Contains(entry.KeepDbKey))))
            {
                throw new AppConflictException("The pending correction is linked to records outside this membership number.");
            }

            var affected = keepKeys.Concat(pending.RetireDbKeys).Where(register.Items.ContainsKey).ToArray();
            if (affected.Any(key => !register.Items[key].Attributes.Any(attribute =>
                attribute.Name == "MembershipNumber" && attribute.Value == number)))
            {
                throw new AppConflictException("A member record no longer has the expected membership number.");
            }
            foreach (var entry in ledger.Entries)
            {
                if (register.Items.TryGetValue(entry.KeepDbKey, out var keepItem) &&
                    register.Items.TryGetValue(entry.RetireDbKey, out var retireItem) &&
                    !MemberMergeCandidates.NamesLookRelated(
                        keepItem.Attributes.First(attribute => attribute.Name == "Name").Value,
                        retireItem.Attributes.First(attribute => attribute.Name == "Name").Value))
                {
                    throw new AppConflictException("The pending correction includes records whose names no longer match.");
                }
            }
            if (pending.Keep.Any(keep => register.Members.Any(other => other.DbKey != keep.DbKey &&
                other.MembershipNumber == request.MembershipNumber && other.SeasonsActive.Intersect(keep.SeasonsActive).Any())))
            {
                throw new AppConflictException("Another member record uses this number in a combined season.");
            }

            using var client = GetClient();
            var memberCondition = new UpdateCondition { Name = "MembershipNumber", Value = number };
            try
            {
                if (!ledger.SeasonsWritten)
                {
                    foreach (var keep in pending.Keep)
                    {
                        var storedSeasons = register.Items[keep.DbKey].Attributes
                            .Where(attribute => attribute.Name == "SeasonsActive").Select(attribute => attribute.Value);
                        var missingSeasons = keep.SeasonsActive.Select(season => ((int)season).ToString(CultureInfo.InvariantCulture))
                            .Except(storedSeasons).ToArray();
                        if (missingSeasons.Length > 0)
                        {
                            await client.PutAttributesAsync(new PutAttributesRequest
                            {
                                DomainName = Domain,
                                ItemName = keep.DbKey,
                                Expected = memberCondition,
                                Attributes = missingSeasons.Select(season =>
                                    new ReplaceableAttribute { Name = "SeasonsActive", Value = season }).ToList()
                            });
                        }
                    }

                    await client.PutAttributesAsync(new PutAttributesRequest
                    {
                        DomainName = Domain,
                        ItemName = ledgerKey,
                        Expected = new UpdateCondition { Name = MergeRevisionAttribute, Value = ledger.Revision },
                        Attributes = new List<ReplaceableAttribute>
                        {
                            new ReplaceableAttribute { Name = MergeSeasonsWrittenAttribute, Value = "1", Replace = true }
                        }
                    });
                }

                foreach (var key in pending.RetireDbKeys)
                {
                    await DeleteMemberAttributes(client, key, memberCondition);
                }

                foreach (var keep in pending.Keep.Where(keep => register.Items[keep.DbKey].Attributes
                    .Any(attribute => attribute.Name == AppliedMergeAttribute)))
                {
                    await DeleteMemberAttributes(client, keep.DbKey, memberCondition,
                        new List<Amazon.SimpleDB.Model.Attribute> { new Amazon.SimpleDB.Model.Attribute { Name = AppliedMergeAttribute } });
                }

                await DeleteMemberAttributes(client, ledgerKey,
                    new UpdateCondition { Name = MergeRevisionAttribute, Value = ledger.Revision });
            }
            catch (AmazonSimpleDBException ex) when (IsConditionFailure(ex))
            {
                throw new AppConflictException("The source correction could not be confirmed. Refresh the pending corrections before continuing.", ex);
            }
        }

        private async Task DeleteMemberAttributes(AmazonSimpleDBClient client, string key, UpdateCondition expected,
            List<Amazon.SimpleDB.Model.Attribute> attributes = null)
        {
            try
            {
                await client.DeleteAttributesAsync(new DeleteAttributesRequest
                {
                    DomainName = Domain,
                    ItemName = key,
                    Expected = expected,
                    Attributes = attributes
                });
            }
            catch (AmazonSimpleDBException ex) when (IsConditionFailure(ex) && attributes == null)
            {
                var item = await client.GetAttributesAsync(new GetAttributesRequest
                {
                    DomainName = Domain,
                    ItemName = key,
                    ConsistentRead = true
                });
                if (item.Attributes?.Count > 0)
                {
                    throw;
                }
            }
        }

        private static bool IsConditionFailure(AmazonSimpleDBException ex)
        {
            return ex.ErrorCode is "ConditionalCheckFailed" or "AttributeDoesNotExist";
        }

        private static PendingMemberMerge DescribePendingMerge(int number, MemberMergeLedger ledger, MemberRegister register)
        {
            var keepKeys = ledger.Entries.Select(entry => entry.KeepDbKey).ToHashSet(StringComparer.Ordinal);
            var keep = register.Members.Where(member => member.MembershipNumber == number && keepKeys.Contains(member.DbKey))
                .Select(ToMergeRecord).ToArray();
            return new PendingMemberMerge(number, keep,
                ledger.Entries.Select(entry => entry.RetireDbKey).Distinct().ToArray(), ledger.Revision);
        }

        private static MemberMergePreview CreatePreview(MemberMergeRequest request, MemberRegister register, MemberMergeLedger ledger)
        {
            var sameNumber = register.Members.Where(member => member.MembershipNumber == request.MembershipNumber).ToList();
            var keep = sameNumber.SingleOrDefault(member => member.DbKey == request.KeepDbKey);
            var retire = sameNumber.SingleOrDefault(member => member.DbKey == request.RetireDbKey);
            if (keep == null || retire == null)
            {
                throw new AppConflictException("Both records must still be in the register and use the selected membership number.");
            }
            if (!MemberMergeCandidates.NamesLookRelated(keep.Name, retire.Name))
            {
                throw new AppConflictException("The selected records do not form a potential split history.");
            }

            var keepRecord = ToMergeRecord(keep);
            var retireRecord = ToMergeRecord(retire);
            var seasons = keepRecord.SeasonsActive.Union(retireRecord.SeasonsActive).OrderBy(season => season).ToArray();
            if (sameNumber.Any(member => member != keep && member != retire && member.SeasonsActive.Intersect(seasons).Any()))
            {
                throw new AppConflictException("A third record already uses this membership number in a combined season.");
            }

            var snapshot = JsonSerializer.SerializeToUtf8Bytes(new { request.MembershipNumber, keepRecord, retireRecord, ledger.Revision });
            return new MemberMergePreview(keepRecord, retireRecord, seasons, Convert.ToHexString(SHA256.HashData(snapshot)));
        }

        private static MemberMergeRecord ToMergeRecord(Member member)
        {
            return new MemberMergeRecord(member.DbKey, member.Name, member.SeasonsActive.Distinct().OrderBy(season => season).ToArray());
        }

        private static void ValidateMergeRequest(MemberMergeRequest request)
        {
            if (request == null || request.MembershipNumber <= 0 || string.IsNullOrWhiteSpace(request.KeepDbKey) ||
                string.IsNullOrWhiteSpace(request.RetireDbKey) || request.KeepDbKey == request.RetireDbKey)
            {
                throw new AppValidationException("Select two different records for the same positive membership number.");
            }
        }

        private static bool WasApplied(MemberMergeLedger ledger, MemberMergeRequest request)
        {
            return ledger.Entries.Any(entry => entry.PreviewToken == request.PreviewToken &&
                entry.KeepDbKey == request.KeepDbKey && entry.RetireDbKey == request.RetireDbKey);
        }

        private static string LedgerKey(int membershipNumber)
        {
            return MergeItemPrefix + membershipNumber.ToString(CultureInfo.InvariantCulture);
        }

        private static MemberMergeLedger GetLedger(MemberMergeRequest request, MemberRegister register)
        {
            return register.Ledgers.TryGetValue(LedgerKey(request.MembershipNumber), out var ledger)
                ? ledger
                : new MemberMergeLedger(null, Array.Empty<MemberMergeEntry>(), false);
        }

        private static Dictionary<string, MemberMergeLedger> ReadMergeLedgers(IEnumerable<Item> items)
        {
            return items.Where(item => item.Name.StartsWith(MergeItemPrefix, StringComparison.Ordinal))
                .ToDictionary(item => item.Name, item => new MemberMergeLedger(
                    item.Attributes.Single(attribute => attribute.Name == MergeRevisionAttribute).Value,
                    item.Attributes.Where(attribute => attribute.Name == MergeEntryAttribute)
                        .Select(attribute => JsonSerializer.Deserialize<MemberMergeEntry>(attribute.Value)).ToArray(),
                    item.Attributes.Any(attribute => attribute.Name == MergeSeasonsWrittenAttribute && attribute.Value == "1")),
                    StringComparer.Ordinal);
        }

        private sealed class StoredMember : Member
        {
            internal HashSet<string> AppliedMerges { get; } = new HashSet<string>(StringComparer.Ordinal);
            internal Season[] LoadedSeasons { get; set; }
        }

        private sealed record MemberMergeEntry(string KeepDbKey, string RetireDbKey, Season[] ImportedSeasons, string PreviewToken);
        private sealed record MemberMergeLedger(string Revision, MemberMergeEntry[] Entries, bool SeasonsWritten);
        private sealed record MemberRegister(List<Member> Members, Dictionary<string, MemberMergeLedger> Ledgers,
            HashSet<string> RetiredKeys, HashSet<string> MergedSurvivorKeys, Dictionary<string, Item> Items);
    }
}
