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

            if (metadata.ContainsKey("Name"))
            {
                Name = metadata["Name"];
            }

            if (metadata.ContainsKey("DoB"))
            {
                DateTime.TryParse(metadata["DoB"], out DateTime dt);
                DoB = dt;
            }

            if (metadata.ContainsKey("PhoneNumber"))
            {
                PhoneNumber = metadata["PhoneNumber"];
            }

            if (metadata.ContainsKey("AllowNameToBeUsed"))
            {
                bool.TryParse(metadata["AllowNameToBeUsed"], out bool dt);
                AllowNameToBeUsed = dt;
            }

            if (metadata.ContainsKey("AcceptPolicies"))
            {
                bool.TryParse(metadata["AcceptPolicies"], out bool dt);
                AcceptPolicies = dt;
            }

            if (metadata.ContainsKey("AcceptPolicies"))
            {
                bool.TryParse(metadata["AcceptPolicies"], out bool dt);
                AcceptPolicies = dt;
            }

            if (metadata.ContainsKey("UnderAge"))
            {
                bool.TryParse(metadata["UnderAge"], out bool dt);
                UnderAge = dt;
            }

            if (metadata.ContainsKey("ParentalConsent"))
            {
                bool.TryParse(metadata["ParentalConsent"], out bool dt);
                ParentalConsent = dt;
            }

            if (metadata.ContainsKey("ChildCanSwim"))
            {
                ChildCanSwim = metadata["ChildCanSwim"];
            }

            if (metadata.ContainsKey("Responsible1st"))
            {
                Responsible1st = metadata["Responsible1st"];
            }

            if (metadata.ContainsKey("Responsible2nd"))
            {
                Responsible2nd = metadata["Responsible2nd"];
            }

            if (metadata.ContainsKey("Responsible3rd"))
            {
                Responsible3rd = metadata["Responsible3rd"];
            }

            if (metadata.ContainsKey("Responsible4th"))
            {
                Responsible4th = metadata["Responsible4th"];
            }

            if (metadata.ContainsKey("EmergencyContact"))
            {
                EmergencyContact = metadata["EmergencyContact"];
            }

            if (metadata.ContainsKey("EmergencyContactPhoneHome"))
            {
                EmergencyContactPhoneHome = metadata["EmergencyContactPhoneHome"];
            }

            if (metadata.ContainsKey("EmergencyContactPhoneWork"))
            {
                EmergencyContactPhoneWork = metadata["EmergencyContactPhoneWork"];
            }

            if (metadata.ContainsKey("EmergencyContactPhoneMobile"))
            {
                EmergencyContactPhoneMobile = metadata["EmergencyContactPhoneMobile"];
            }
        }

        public DateTime ValidOn { get; set; }
        public int MembershipNumber { get; set; }
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
