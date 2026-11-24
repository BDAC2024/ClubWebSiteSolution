using AnglingClubShared.Enums;

namespace AnglingClubShared.DTOs
{
    public record MembershipTrendsDto(DateOnly AsOfDate, IReadOnlyList<MembershipSeasonComparisonDto> Comparisons);

    public record MembershipSeasonComparisonDto(
        Season PreviousSeason,
        Season Season,
        bool IsProvisional,
        int PreviousMembers,
        int Members,
        int Renewed,
        int DidNotRenew,
        int FirstRecordedMembers,
        int ReturnedAfterGap)
    {
        public int NetChange => Members - PreviousMembers;
        public decimal? ChurnRate => PreviousMembers == 0 ? null : (decimal)DidNotRenew / PreviousMembers;
    }
}
