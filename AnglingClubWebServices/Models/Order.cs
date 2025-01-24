using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
{
    public class Order : TableBase
    {
        public int OrderId { get; set; }
        public PaymentType OrderType { get; set; }
        public string Description { get; set; }
        public int TicketNumber { get; set; }
        public string MembersName { get; set; } = "";
        public string GuestsName { get; set; } = "";
        public string TicketHoldersName { get; set; } = "";
        public DateTime? ValidOn { get; set; } = null;
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }

        public DateTime? PaidOn { get; set; } = null;
        public DateTime? IssuedOn { get; set; } = null;

        // Note: Internal get means this wont be sent back from API call to clientpublic string PaymentId { get; set; }
        public string PaymentId { internal get; set; }
        public string Status { get; set; }

        public Season Season 
        {
            get
            {
                DateTime? dateToUse;

                switch (OrderType)
                {
                    case PaymentType.Membership:
                        dateToUse = PaidOn;
                        break;
                    case PaymentType.GuestTicket:
                        dateToUse = ValidOn;
                        break;
                    case PaymentType.DayTicket:
                        dateToUse = ValidOn;
                        break;
                    default:
                        dateToUse = PaidOn;
                        break;
                }

                return dateToUse.HasValue ? EnumUtils.SeasonForDate(dateToUse.Value).Value : Season.Unknown;
            }

        }
    }
}
