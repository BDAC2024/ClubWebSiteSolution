using AnglingClubShared.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnglingClubWebServices.Models
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


        // Note: Internal get means this wont be sent back from API call to client
        public string ProductDayTicket { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string ProductGuestTicket { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string ProductPondGateKey { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string ProductHandlingCharge { internal get; set; }
    }

    public class AppSetting : TableBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
