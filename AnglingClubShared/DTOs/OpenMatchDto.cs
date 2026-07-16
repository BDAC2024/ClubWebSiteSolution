using AnglingClubShared.Enums;

namespace AnglingClubShared.DTOs
{
    public class OpenMatchDto
    {
        public string DbKey { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime Draw { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int PegsAvailable { get; set; }
        public int PegsRemaining { get; set; }
        public Season Season { get; set; }
        public OpenMatchType OpenMatchType { get; set; }
        public string DrawTime { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool InThePast { get; set; }
    }
}
