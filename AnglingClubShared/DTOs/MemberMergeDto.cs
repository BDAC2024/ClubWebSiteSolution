using AnglingClubShared.Enums;

namespace AnglingClubShared.DTOs
{
    public record MemberMergeCandidate(int MembershipNumber, string[] Names);

    public record MemberMergeRecord(string DbKey, string Name, Season[] SeasonsActive);

    public record MemberMergePair(string FirstDbKey, string SecondDbKey);

    public record MemberMergeRecords(MemberMergeRecord[] Records, MemberMergePair[] EligiblePairs);

    public record MemberMergeRequest(
        int MembershipNumber, string KeepDbKey, string RetireDbKey, string? PreviewToken = null);

    public record MemberMergePreview(
        MemberMergeRecord Keep, MemberMergeRecord Retire, Season[] CombinedSeasonsActive, string PreviewToken);

    public record PendingMemberMerge(int MembershipNumber, MemberMergeRecord[] Keep, string[] RetireDbKeys, string Revision);

    public record FinalizeMemberMergeRequest(int MembershipNumber, string Revision);
}
