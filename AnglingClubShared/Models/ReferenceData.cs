using AnglingClubShared.Enums;

namespace AnglingClubShared.Models
{
    public class ReferenceData
    {
        public Season CurrentSeason { get; set; }

        public List<SeasonInfo> Seasons { get; set; } = new List<SeasonInfo>();

        public AppSettings AppSettings { get; set; } = new AppSettings();
    }

    public class SeasonInfo
    {
        public Season Season { get; set; }
        public string Name { get; set; } = "";
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
    }

}
