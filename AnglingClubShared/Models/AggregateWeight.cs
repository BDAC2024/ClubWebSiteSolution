using AnglingClubShared.Extensions;

namespace AnglingClubShared.Models
{

    public class AggregateWeight
    {
        public int Position { get; set; }
        public string PositionOrdinal {
            get
            {
                return Position.Ordinal();

            }

        }

        public string Name { get; set; } = "";
        public float TotalWeightDecimal { get; set; }

        public int MembershipNumber { get; set; }
        public string Weight {
            get
            {
                return TotalWeightDecimal.WeightAsString();
            }

        }

        // Info related to dropping matches
        public int MatchesInSeason { get; set; }
        public float DroppedWeightDecimal { get; set; }

        public string DroppedWeight {
            get
            {
                if (DroppedWeightDecimal == 0)
                {
                    return "0";
                }
                return DroppedWeightDecimal.WeightAsString();
            }

        }

    }

}
