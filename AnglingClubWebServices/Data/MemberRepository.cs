using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public partial class MemberRepository : RepositoryBase, IMemberRepository
    {
        private const string IDPREFIX = "Member";
        private readonly ILogger<MemberRepository> _logger;


        public MemberRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MemberRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get; }

        public async Task AddOrUpdateMember(Member member)
        {
            using var client = GetClient();

            var isNew = member.IsNewItem;
            if (isNew)
            {
                member.DbKey = member.GenerateDbKey(IDPREFIX);
            }

            var register = await ReadRegister();
            if (register.RetiredKeys.Contains(member.DbKey))
            {
                throw new AppConflictException("This member record has been retired by a merge.");
            }
            if (register.MergedSurvivorKeys.Contains(member.DbKey))
            {
                throw new AppConflictException("Finish the pending source correction before editing this member.");
            }
            var existing = register.Members.SingleOrDefault(row => row.DbKey == member.DbKey);
            if (!isNew && existing == null)
            {
                throw new AppConflictException("This member record no longer exists. Reload the member register.");
            }
            var seasonsChanged = member is not StoredMember stored || !stored.LoadedSeasons.ToHashSet().SetEquals(member.SeasonsActive);
            if (seasonsChanged && member is StoredMember loaded && !loaded.LoadedSeasons.ToHashSet().SetEquals(existing.SeasonsActive))
            {
                throw new AppConflictException("The member's seasons have changed. Reload the member register.");
            }

            // Check that there aren't already members of this membership number in each active season
            foreach (var season in member.SeasonsActive)
            {
                if (register.Members.Any(x => x.MembershipNumber == member.MembershipNumber && x.SeasonsActive.Contains(season) && x.DbKey != member.DbKey))
                {
                    throw new Exception($"Membership number {member.MembershipNumber} is already active for {season.SeasonName()}");
                }
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Name", Value = member.Name, Replace = true },
                new ReplaceableAttribute { Name = "Email", Value = member.Email?? "", Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = member.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Admin", Value = member.Admin ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "Treasurer", Value = member.Treasurer ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "CommitteeMember", Value = member.CommitteeMember ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "Secretary", Value = member.Secretary ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "MembershipSecretary", Value = member.MembershipSecretary ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "MatchSecretary", Value = member.MatchSecretary ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "Previewer", Value = member.Previewer ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "InitialPin", Value = member.InitialPin.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Pin", Value = member.Pin, Replace = true },
                new ReplaceableAttribute { Name = "AllowNameToBeUsed", Value = member.AllowNameToBeUsed ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "PreferencesLastUpdated", Value = dateOffsetToString(member.PreferencesLastUpdated), Replace = true },
                new ReplaceableAttribute { Name = "PinResetRequired", Value = member.PinResetRequired ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "PinResetRequested", Value = member.PinResetRequested ? "1" : "0", Replace = true },
                new ReplaceableAttribute { Name = "FailedLoginAttempts", Value = member.FailedLoginAttempts.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "LastLoginFailure", Value = dateToString(member.LastLoginFailure), Replace = true },
                new ReplaceableAttribute { Name = "ReLoginRequired", Value = member.ReLoginRequired ? "1" : "0", Replace = true },
            };

            foreach (var season in seasonsChanged ? member.SeasonsActive : Enumerable.Empty<Season>())
            {
                attributes.Add(new ReplaceableAttribute { Name = "SeasonsActive", Value = ((int)season).ToString(), Replace = true });
            }

            try
            {
                if (isNew)
                {
                    SetupTableAttribues(request, member.DbKey, attributes);
                    await WriteInBatches(request, client);
                }
                else
                {
                    var storedNumber = register.Items[member.DbKey].Attributes.Single(attribute => attribute.Name == "MembershipNumber").Value;
                    await client.PutAttributesAsync(new PutAttributesRequest
                    {
                        DomainName = Domain,
                        ItemName = member.DbKey,
                        Attributes = attributes,
                        Expected = new UpdateCondition { Name = "MembershipNumber", Value = storedNumber }
                    });
                    await BackupAfterWrite(client);
                }
                _logger.LogDebug($"Member added: {member.DbKey} - {member.Name}");

            }
            catch (AmazonSimpleDBException ex) when (IsConditionFailure(ex))
            {
                throw new AppConflictException("The member record changed or was retired. Reload the member register.", ex);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Member>> GetMembers(Season? activeSeason = null, bool forMatchResults = false)
        {
            var members = (await ReadRegister()).Members;

            if (forMatchResults)
            {
                foreach (var member in members.Where(member => !member.AllowNameToBeUsed))
                {
                    member.Name = "Anonymous";
                }
            }

            return activeSeason.HasValue
                ? members.Where(member => member.SeasonsActive.Contains(activeSeason.Value)).ToList()
                : members;
        }

        private async Task<MemberRegister> ReadRegister()
        {
            var items = await GetData(IDPREFIX, "AND Name > ''", "ORDER BY Name", consistentRead: true);
            var ledgers = ReadMergeLedgers(items);
            var merges = ledgers.Values.SelectMany(ledger => ledger.Entries).ToList();
            var retiredKeys = merges.Select(merge => merge.RetireDbKey).ToHashSet(StringComparer.Ordinal);
            var pendingSeasonsBySurvivor = ledgers.Values.Where(ledger => !ledger.SeasonsWritten)
                .SelectMany(ledger => ledger.Entries).ToLookup(merge => merge.KeepDbKey, StringComparer.Ordinal);
            var members = new List<Member>();

            foreach (var item in items)
            {
                if (item.Name.StartsWith(MergeItemPrefix, StringComparison.Ordinal) || retiredKeys.Contains(item.Name))
                {
                    continue;
                }

                var member = new StoredMember { DbKey = item.Name };

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Name":
                            member.Name = attribute.Value;
                            break;

                        case "Email":
                            member.Email = attribute.Value;
                            break;

                        case "MembershipNumber":
                            member.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "Admin":
                            member.Admin = attribute.Value == "0" ? false : true;
                            break;

                        case "Treasurer":
                            member.Treasurer = attribute.Value == "0" ? false : true;
                            break;

                        case "CommitteeMember":
                            member.CommitteeMember = attribute.Value == "0" ? false : true;
                            break;

                        case "Secretary":
                            member.Secretary = attribute.Value == "0" ? false : true;
                            break;

                        case "MembershipSecretary":
                            member.MembershipSecretary = attribute.Value == "0" ? false : true;
                            break;

                        case "MatchSecretary":
                            member.MatchSecretary = attribute.Value == "0" ? false : true;
                            break;

                        case "Previewer":
                            member.Previewer = attribute.Value == "0" ? false : true;
                            break;

                        case "SeasonsActive":
                            member.SeasonsActive.Add((Season)Convert.ToInt32(attribute.Value));
                            break;

                        case AppliedMergeAttribute:
                            if (!string.IsNullOrEmpty(attribute.Value))
                            {
                                member.AppliedMerges.Add(attribute.Value);
                            }
                            break;

                        case "Pin":
                            member.Pin = attribute.Value;
                            break;

                        case "InitialPin":
                            member.InitialPin = attribute.Value != "" ? Convert.ToInt32(attribute.Value) : 0;
                            break;

                        case "PinResetRequired":
                            member.PinResetRequired = attribute.Value == "0" ? false : true; ;
                            break;

                        case "PinResetRequested":
                            member.PinResetRequested = attribute.Value == "0" ? false : true; ;
                            break;

                        case "AllowNameToBeUsed":
                            member.AllowNameToBeUsed = attribute.Value == "1";
                            break;

                        case "PreferencesLastUpdated":
                            member.PreferencesLastUpdated = DateTime.Parse(attribute.Value);
                            break;

                        case "FailedLoginAttempts":
                            member.FailedLoginAttempts = Convert.ToInt32(attribute.Value);
                            break;

                        case "LastLoginFailure":
                            member.LastLoginFailure = DateTime.Parse(attribute.Value);
                            break;

                        case "ReLoginRequired":
                            member.ReLoginRequired = attribute.Value == "0" ? false : true;
                            break;

                        default:
                            break;
                    }
                }

                foreach (var merge in pendingSeasonsBySurvivor[member.DbKey])
                {
                    if (member.AppliedMerges.Add(merge.RetireDbKey))
                    {
                        member.SeasonsActive = member.SeasonsActive.Union(merge.ImportedSeasons).OrderBy(season => season).ToList();
                    }
                }

                member.LoadedSeasons = member.SeasonsActive.ToArray();
                members.Add(member);
            }

            return new MemberRegister(members, ledgers, retiredKeys,
                merges.Select(merge => merge.KeepDbKey).ToHashSet(StringComparer.Ordinal),
                items.ToDictionary(item => item.Name, StringComparer.Ordinal));

        }


    }
}
