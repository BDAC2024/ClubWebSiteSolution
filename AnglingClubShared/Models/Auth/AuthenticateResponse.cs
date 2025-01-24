
using AnglingClubShared.Entities;

namespace AnglingClubShared.Models.Auth
{
    public class AuthenticateResponse
    {
        public string Id { get; set; } = "";
        //public int MembershipNumber { get; set; }
        //public string Name { get; set; }
        public string Token { get; set; } = "";
        public int ExpiresIn { get; set; }
        public DateTime Expiration { get; set; }

        public AuthenticateResponse()
        {

        }

        public AuthenticateResponse(string memberId, string token, DateTime expiration, int expiresIn)
        {
            Id = memberId;
            //MembershipNumber = member.MembershipNumber;
            //Name = member.Name;
            Token = token;
            Expiration = expiration;
            ExpiresIn = expiresIn;

        }
    }
}
