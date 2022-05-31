using AnglingClubWebServices.Helpers;
using System;

namespace AnglingClubWebServices.Models
{

    public class LeaguePosition
    {
        public int Position { get; set; }
        public string PositionOrdinal
        {
            get
            {
                return Position.Ordinal();

            }

        }

        public string Name { get; set; }
        public int MembershipNumber { get; set; }
        public float Points { get; set; }
        public float TotalWeightDecimal { get; set; }

        public string Weight
        {
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
