using System;

namespace AnglingClubWebServices.DTOs
{
    public class OrderDetailDto
    {
        public string Description { get; set; }
        public string Address { get; set; }

        public DateTime? CreatedOn { get; set; }

        public decimal Amount { get; set; }
        public string Status { get; set; }

        public DateTime? ValidOn { get; set; }
        public int MembershipNumber { get; set; }
        public int TicketNumber { get; set; }
        public string TicketHoldersName { get; set; }
        public string MembersName { get; set; }
        public string GuestsName { get; set; }
        public string Name { get; set; }
        public DateTime DoB { get; set; }
        public string PhoneNumber { get; set; }
        public bool AllowNameToBeUsed { get; set; }
        public bool AcceptPolicies { get; set; }
        public bool UnderAge { get; set; }
        public bool ParentalConsent { get; set; }
        public string ChildCanSwim { get; set; }

        public string Responsible1st { get; set; }
        public string Responsible2nd { get; set; }
        public string Responsible3rd { get; set; }
        public string Responsible4th { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyContactPhoneHome { get; set; }
        public string EmergencyContactPhoneWork { get; set; }
        public string EmergencyContactPhoneMobile { get; set; }
    }
}
