namespace AnglingClubShared.DTOs
{
    public class MemberInitialPinDto
    {
        public int MembershipNumber { get; set; }
        public int InitialPin { get; set; }
        public string Name { get; set; } // Is not stored, just used to log issues

    }
}
