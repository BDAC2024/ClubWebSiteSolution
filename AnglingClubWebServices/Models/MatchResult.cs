using AnglingClubWebServices.Helpers;
using System;

namespace AnglingClubWebServices.Models
{

    public class MatchResultBase : TableBase
    {
        public string MatchId { get; set; }
        public int MembershipNumber { get; set; }
        public string Peg { get; set; }
        public float Points { get; set; }
    }

    public class MatchResult : MatchResultBase
    {
        public float WeightDecimal { get; set; }

        public string Weight
        {
            get
            {
                var wt = "DNW";

                if (WeightDecimal > 0)
                {
                    var wtLb = Math.Floor(this.WeightDecimal);
                    var wtOz = Math.Round((this.WeightDecimal - wtLb) * 16);
                    wt = $"{wtLb}lb {wtOz}oz";
                }

                return wt;

            }

        }

        public int Position { get; set; }

        public string PositionOrdinal
        {
            get
            {
                return Position.Ordinal();

            }

        }

    }


}
