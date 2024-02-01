using AnglingClubWebServices.Interfaces;
using System;

namespace AnglingClubWebServices.Models
{
    public class ProductMembership : TableBase
    {
        public MembershipType Type { get; set; }
        public string Description { get; set; }
        public string Term { get; set; }
        public string Runs { get; set; }
        public decimal Cost { get; set; }

        // Note: Internal get means this wont be sent back from API call to client
        public string Product { internal get; set; }
    }
}
