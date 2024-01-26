using AnglingClubWebServices.Interfaces;
using Stripe;
using System;
using System.Collections.Generic;

namespace AnglingClubWebServices.Models
{
    public class Payment
    {
        public string SessionId { get; set; }
        public PaymentType Category { get; set; }
        public string Purchase { get; set; }
        public string MembersName { get; set; }
        public string HoldersName { get; set; }
        public string GuestsName { get; set; }

        public string Email { get; set; }
        public string CardHoldersName { get; set; }
        public string ShippingAddress { get; set; }
        public double Amount { get; set; }
        public DateTime PaidOn { get; set; }
        public DateTime ValidOn { get; set; }
        public string Status { get; set; }
        public int MembershipNumber { get; internal set; }
    }

    public class PaymentMetaData
    {
        public PaymentMetaData()
        {

        }

        public PaymentMetaData(Dictionary<string, string> metadata)
        {
            if (metadata.ContainsKey("ValidOn"))
            {
                DateTime.TryParse(metadata["ValidOn"], out DateTime dt);
                ValidOn = dt;
            }

            if (metadata.ContainsKey("MembershipNumber"))
            {
                if (int.TryParse(metadata["MembershipNumber"], out int mn))
                {
                    MembershipNumber = mn;
                }
            }

            if (metadata.ContainsKey("HoldersName"))
            {
                TicketHoldersName = metadata["HoldersName"];
            }

            if (metadata.ContainsKey("MembersName"))
            {
                MembersName = metadata["MembersName"];
            }

            if (metadata.ContainsKey("GuestsName"))
            {
                GuestsName = metadata["GuestsName"];
            }
        }

        public DateTime ValidOn { get; set; }
        public int MembershipNumber { get; set; }
        public string TicketHoldersName { get; set; }
        public string MembersName { get; set; }
        public string GuestsName { get; set; }

    }
}
