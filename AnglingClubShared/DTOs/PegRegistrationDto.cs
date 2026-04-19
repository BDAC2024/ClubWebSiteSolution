using AnglingClubShared.Entities;
using AnglingClubShared.Enums;

namespace AnglingClubShared.DTOs
{
    public class PegRegistrationRequestDto
    {
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public Season Season { get; set; }
    }

    public class PegRegistrationOutputDto
    {
        public string DbKey { get; set; } = "";
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public Season Season { get; set; }
        public int MembershipNumber { get; set; }
        public string Name { get; set; } = "";
        public DateTime DateRegistered { get; set; }
    }

    public class PegAllocationRequestDto
    {
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public int MembershipNumber { get; set; }
        public DateOnly DateAllocated { get; set; }
    }

    public class PegAllocationOutputDto
    {
        public string DbKey { get; set; } = "";
        public string Stretch { get; set; } = "";
        public string Peg { get; set; } = "";
        public Season Season { get; set; }
        public int MembershipNumber { get; set; }
        public string Name { get; set; } = "";
        public DateOnly DateAllocated { get; set; }
    }
}
