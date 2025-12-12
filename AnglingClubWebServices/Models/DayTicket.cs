using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
{
    public class DayTicket : TableBase
    {
        public int TicketNumber { get; set; }
        public string PaymentId { get; set; }
        public DateTime? IssuedOn { get; set; } = null;

        public Season Season 
        {
            get
            {
                return IssuedOn.HasValue ? EnumUtils.SeasonForDate(IssuedOn.Value).Value : Season.Unknown;
            }

        }
    }
}
