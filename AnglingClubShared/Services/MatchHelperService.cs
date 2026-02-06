using AnglingClubShared.Enums;

namespace AnglingClubShared.Services
{
    public class MatchHelperService
    {
        public static int MatchesToBeDropped(AggregateType aggType, Season season)
        {
            var drop = 0;

            if (season >= Season.S25To26)
            {
                if (aggType == AggregateType.ClubRiver)
                {
                    drop = 10;
                }

            }

            return drop;
        }
    }
}
