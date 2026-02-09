using AnglingClubShared.Extensions;

namespace AnglingClubShared.Models
{

    public class LeaguePosition
    {
        public int Position { get; set; }
        public string PositionOrdinal {
            get
            {
                return Position.Ordinal();

            }

        }

        public string Name { get; set; } = "";
        public int MembershipNumber { get; set; }
        public float Points { get; set; }
        public float TotalWeightDecimal { get; set; }

        public string Weight {
            get
            {
                return TotalWeightDecimal.WeightAsString();
            }

        }

        // Info related to dropping matches
        public int MatchesInSeason { get; set; }
        public float DroppedPoints { get; set; }


    }

}
