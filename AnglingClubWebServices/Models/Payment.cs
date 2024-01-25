using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
{
    public class Payment
    {
        public string SessionId { get; set; }
        public PaymentType Category { get; set; }
        public string Purchase { get; set; }
        public string MembersName { get; set; }
        public string HoldersName { get; set; }
        public string Email { get; set; }
        public string CardHoldersName { get; set; }
        public string ShippingAddress { get; set; }
        public double Amount { get; set; }
        public DateTime PaidOn { get; set; }
        public DateTime ValidOn { get; set; }
        public string Status { get; set; }

    }
}
