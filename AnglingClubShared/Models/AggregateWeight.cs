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
    }

}
