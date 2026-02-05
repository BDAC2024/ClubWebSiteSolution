using AnglingClubShared.Enums;
using MatchType = AnglingClubShared.Enums.MatchType;

namespace AnglingClubWebsite.Models
{
    public class MatchTabData
    {
        public string HeaderFull { get; set; } = "";
        public string HeaderBrief { get; set; } = "";
        public MatchType MatchType { get; set; } = MatchType.Spring;
        public AggregateType AggregateType { get; set; } = AggregateType.Spring;
        public bool Visible { get; set; } = false;
    }

}
