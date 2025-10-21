using AnglingClubWebServices.Models;
using System;

namespace AnglingClubWebServices.DTOs
{
    public class NewMembershipDto
    {
        public string DbKey { get; set; }
        public string SeasonName { get; set; }
        public string Name { get; set; }
        public DateTime DoB { get; set; }
        public string PhoneNumber { get; set; }
        public bool AllowNameToBeUsed { get; set; }
        public bool AcceptPolicies { get; set; }
        public bool UnderAge { get; set; }
        public bool PaidForKey { get; set; }
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

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }

    }
}
