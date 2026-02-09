using AnglingClubShared.Extensions;

namespace AnglingClubShared.Entities
{

    public class MatchResultBase : TableBase
    {
        public string MatchId { get; set; } = "";
        public int MembershipNumber { get; set; }
        public string Peg { get; set; } = "";
        public float Points { get; set; }
    }

    public class MatchResult : MatchResultBase
    {
        public float WeightDecimal { get; set; }

        public string Weight {
            get
            {
                return WeightDecimal.WeightAsString();
            }

        }

        public int Position { get; set; }

        public string PositionOrdinal {
            get
            {
                return Position.Ordinal();

            }

        }

    }


}
