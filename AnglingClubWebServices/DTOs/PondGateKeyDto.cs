namespace AnglingClubShared.DTOs
{
    public class PondGateKeyDto
    {
        public string DbKey { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public bool AcceptPolicies { get; set; }
        public bool PotentialMember { get; set; }

        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }

    }
}
