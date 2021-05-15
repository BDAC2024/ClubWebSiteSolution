namespace AnglingClubWebServices.Models
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public int MembershipNumber { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(Member member, string token)
        {
            Id = member.DbKey;
            MembershipNumber = member.MembershipNumber;
            Name = member.Name;
            Token = token;
        }
    }
}
