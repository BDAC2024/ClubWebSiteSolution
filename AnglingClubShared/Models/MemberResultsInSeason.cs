using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;

namespace AnglingClubShared.Models
{
    public class MemberResultsInSeason
    {
        public Season Season { get; set; }
        public int MembershipNumber { get; set; }
        public string MemberName { get; set; } = "";
        public AggregateType AggregateType { get; set; }
        public int MatchesInSeason { get; set; }
        public int MatchesFished { get; set; }
        public int MatchesDropped { get; set; }
        public float CountedWeightDecimal { get; set; }
        public string CountedWeight {
            get
            {
                return CountedWeightDecimal.WeightAsString();
            }
        }
        public float DroppedWeightDecimal { get; set; }
        public string DroppedWeight {
            get
            {
                return DroppedWeightDecimal.WeightAsString();
            }
        }

        public float CountedPoints { get; set; }
        public float DroppedPoints { get; set; }

        public List<MatchAllResultOutputDto> ResultsCounted { get; set; } = new List<MatchAllResultOutputDto>();
        public List<MatchAllResultOutputDto> ResultsDropped { get; set; } = new List<MatchAllResultOutputDto>();
    }
}
