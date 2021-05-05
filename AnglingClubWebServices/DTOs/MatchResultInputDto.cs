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


}
