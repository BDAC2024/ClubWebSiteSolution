using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Services;
using System;
using System.Security.Cryptography;

namespace AnglingClubWebServices.Models
{
    public class Member : TableBase
    {

        public string Name { get; set; }
        public int MembershipNumber { get; set; }
        public bool Admin { get; set; } = false;
        public DateTime LastPaid { get; set; }
        public bool Enabled { get; set; } = true;
        public string Pin { get; set; }
        public bool PinResetRequired { get; set; } = true;
        public bool AllowNameToBeUsed { get; set; } = false;
        public DateTime PreferencesLastUpdated { get; set; } = DateTime.MinValue;
        public DateTime LastLoginFailure { get; set; } = DateTime.MinValue;
        public int FailedLoginAttempts { get; set; }

        public Season Season
        {
            get
            {
                var prevSeasonOffset = LastPaid.Month < 3 || (LastPaid.Day == 3 && LastPaid.Day < 15) ? 1 : 0;

                return (Season)(LastPaid.Year - 2000 - prevSeasonOffset);
            }
        }

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
            
            var hashedPin = AuthService.HashText(newPin.ToString(), DbKey, new SHA1CryptoServiceProvider());
            Pin = hashedPin;

            return newPin;
        }

        public bool ValidPin(int pinToCheck)
        {
            var hashedPinToCheck = AuthService.HashText(pinToCheck.ToString(), DbKey, new SHA1CryptoServiceProvider());

            return hashedPinToCheck == Pin;
        }
    }
}
