using AnglingClubWebServices.Models;

namespace AnglingClubWebServices.DTOs
{
    public class MatchResultInputDto : MatchResultBase
    {
        public int Lb { get; set; }
        public float Oz { get; set; }

        public float WeightDecimal
        {
            get
            {
                float wt = (Lb * 1f + Oz / 16f);

                return wt;

            }

        }
    }

    public class MatchResultOutputDto : MatchResult
    {
        public string Name { get; set; }
    }

    public class MatchAllResultOutputDto : MatchResult
    {
        public string Name { get; set; }
        public string MatchType { get; set; }
        public string AggType { get; set; } 
        public string Season { get; set; }
        public string Venue { get; set; }
    }

}
