using System;

namespace AnglingClubWebServices.Models
{
    public class Member : TableBase
    {
        public string Name { get; set; }
        public int MembershipNumber { get; set; }
        public bool Admin { get; set; } = false;
        public DateTime LastPaid { get; set; }
        public bool Enabled { get; set; } = true;
        public int Pin { get; set; }
        public bool AllowNameToBeUsed { get; set; } = false;
        public DateTime PreferencesLastUpdated { get; set; } = DateTime.MinValue;
    }
}
