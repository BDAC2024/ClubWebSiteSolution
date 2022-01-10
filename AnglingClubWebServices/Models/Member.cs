using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AnglingClubWebServices.Models
{
    public class Member : TableBase
    {

        public string Name { get; set; }
        public string Email { get; set; }
        public int MembershipNumber { get; set; }
        public bool Admin { get; set; } = false;
        public string Pin { get; set; }
        public bool PinResetRequested { get; set; } = false;
        public bool PinResetRequired { get; set; } = true;
        public bool AllowNameToBeUsed { get; set; } = false;
        public DateTime PreferencesLastUpdated { get; set; } = DateTime.MinValue;
        public DateTime LastLoginFailure { get; set; } = DateTime.MinValue;
        public int FailedLoginAttempts { get; set; } = 0;

        public List<Season> SeasonsActive { get; set; } = new List<Season>();
        public bool ReLoginRequired { get; set; } = false;

        public int NewPin(int? toPin = null)
        {
            int newPin;

            // If PIN in NOT supplied, must be an admin doing a PIN reset, otherwise its a user changing their own PIN
            if (toPin == null)
            {
                newPin = toPin != null ? toPin.Value : new Random().Next(8999) + 1000;
                PinResetRequired = true;
            }
            else
            {
                PinResetRequired = false;
                newPin = toPin.Value;
            }
            
            var hashedPin = AuthService.HashText(newPin.ToString(), MembershipNumber.ToString(), new SHA1CryptoServiceProvider());
            Pin = hashedPin;

            return newPin;
        }

        public bool ValidPin(int pinToCheck)
        {
            var hashedPinToCheck = AuthService.HashText(pinToCheck.ToString(), MembershipNumber.ToString(), new SHA1CryptoServiceProvider());

            return hashedPinToCheck == Pin;
        }

    }
}
