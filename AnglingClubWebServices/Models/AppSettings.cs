using System;
using System.Collections.Generic;

namespace AnglingClubWebServices.Models
{
    public class AppSettings : TableBase
    {
        public decimal GuestTicketCost { get; set; }
        public List<int> Previewers { get; set; } = new List<int>();


        public decimal DayTicketCost { get; set; }
        public string DayTicketStyle { get; set; }
        public string DayTicket { get; set; }

    }
}
