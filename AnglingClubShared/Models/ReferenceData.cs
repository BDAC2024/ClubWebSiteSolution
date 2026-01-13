using AnglingClubShared.Entities;
using AnglingClubShared.Enums;

namespace AnglingClubShared.Models
{
    public class ReferenceData
    {
        public Season CurrentSeason { get; set; }

        public List<SeasonInfo> Seasons { get; set; } = new List<SeasonInfo>();
        public List<SeasonInfo> SeasonsForMembershipPurchase { get; set; } = new List<SeasonInfo>();

        /// <summary>
        /// Matches that are scheduled on day ticket waters.
        /// </summary>
        public List<ClubEvent> DayTicketMatches { get; set; } = new List<ClubEvent>();

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
