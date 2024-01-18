using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
{
    public class GuestTicket : TableBase
    {
        public int TicketNumber { get; set; }
        public decimal Cost { get; set; }
        public string IssuedBy { get; set; }
        public int IssuedByMembershipNumber { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime TicketValidOn { get; set; }
        public string MembersName { get; set; }
        public int MembershipNumber { get; set; }
        public string EmailTo { get; set; }
        public string GuestsName { get; set; }
        public string ImageData { get; set; }

        public Season Season 
        {
            get
            {
                return EnumUtils.SeasonForDate(TicketValidOn).Value;
            }

        }
    }
}
