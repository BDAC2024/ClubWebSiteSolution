using AnglingClubShared.Entities;
using System.Text.Json.Serialization;

namespace AnglingClubShared.Models
{
    public class AppSettings
    {
        public decimal GuestTicketCost { get; set; }
        public decimal DayTicketCost { get; set; }
        public decimal PondGateKeyCost { get; set; }
        public decimal HandlingCharge { get; set; }

        public List<int> Previewers { get; set; } = new List<int>();
        public List<int> MembershipSecretaries { get; set; } = new List<int>();
        public List<int> Treasurers { get; set; } = new List<int>();

        public bool MembershipsEnabled { get; set; } = false;
        public bool GuestTicketsEnabled { get; set; } = false;
        public bool DayTicketsEnabled { get; set; } = false;
        public bool PondGateKeysEnabled { get; set; } = false;


        [JsonIgnore]
        public string ProductDayTicket { get; set; } = "";

        [JsonIgnore]
        public string ProductGuestTicket { get; set; } = "";

        [JsonIgnore]
        public string ProductPondGateKey { get; set; } = "";

        [JsonIgnore]
        public string ProductHandlingCharge { get; set; } = "";
    }

    public class AppSetting : TableBase
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
