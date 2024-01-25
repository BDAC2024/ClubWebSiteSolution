using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnglingClubWebServices.Models
{
    public class AppSettings
    {
        public decimal GuestTicketCost { get; set; }
        public List<int> Previewers { get; set; } = new List<int>();


        public decimal DayTicketCost { get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string DayTicketStyle { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string DayTicket { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string ProductDayTicket { internal get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string ProductGuestTicket { internal get; set; }
        
    }

    public class AppSetting : TableBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
