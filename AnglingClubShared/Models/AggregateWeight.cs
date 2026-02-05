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
                var wt = "";

                if (TotalWeightDecimal > 0)
                {
                    var wtLb = Math.Floor(this.TotalWeightDecimal);
                    var wtOz = Math.Round((this.TotalWeightDecimal - wtLb) * 16);
                    wt = $"{wtLb}lb {wtOz}oz";
                }

                return wt;

            }

        }

        // Info related to dropping matches
        public int MatchesInSeason { get; set; }
        public int FishedMatches { get; set; }
        public int DroppedMatches { get; set; }
        public float DroppedWeightDecimal { get; set; }

        public string DroppedWeight {
            get
            {
                var wt = "0";

                if (DroppedWeightDecimal > 0)
                {
                    var wtLb = Math.Floor(this.DroppedWeightDecimal);
                    var wtOz = Math.Round((this.DroppedWeightDecimal - wtLb) * 16);
                    wt = $"{wtLb}lb {wtOz}oz";
                }

                return wt;

            }

        }

    }

}
