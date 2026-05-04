using AnglingClubShared.Enums;

namespace AnglingClubShared.Entities
{
    public class PegRegistration : TableBase
    {
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public Season Season { get; set; }
        public int MembershipNumber { get; set; }
        public DateTime DateRegistered { get; set; }
    }

    public class PegAllocation : TableBase
    {
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public Season Season { get; set; }
        public int MembershipNumber { get; set; }
        public DateOnly DateAllocated { get; set; }
    }
}
