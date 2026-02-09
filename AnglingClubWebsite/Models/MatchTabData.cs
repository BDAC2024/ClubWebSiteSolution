using AnglingClubShared.Enums;
using MatchType = AnglingClubShared.Enums.MatchType;

namespace AnglingClubWebsite.Models
{
    public class TabData
    {
        public string HeaderFull { get; set; } = "";
        public string HeaderBrief { get; set; } = "";
        public MatchType MatchType { get; set; } = MatchType.Spring;
        public AggregateType AggregateType { get; set; } = AggregateType.Spring;
        public TrophyType TrophyType { get; set; } = TrophyType.Senior;
        public bool Visible { get; set; } = false;
    }

}
