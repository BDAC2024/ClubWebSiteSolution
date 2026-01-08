using AnglingClubShared.Entities;
using AnglingClubShared.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace AnglingClubShared.DTOs
{
    public class ClientMemberDto
    {
        public string Id { get; set; } = "";
        public string MembershipNumber { get; set; } = "";
        public bool Admin { get; set; } = false;
        public bool Treasurer { get; set; } = false;
        public bool CommitteeMember { get; set; } = false;
        public bool Secretary { get; set; } = false;
        public bool MembershipSecretary { get; set; } = false;
        public bool Previewer { get; set; } = false;
        public bool Developer { get; set; } = false;
        public bool AllowNameToBeUsed { get; set; } = false;
        public DateTime PreferencesLastUpdated { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public bool PinResetRequired { get; set; } = false;

        public ClientMemberDto()
        {

        }

        public ClientMemberDto(JwtSecurityToken token)
        {
            this.Id = token.Claims.First(claim => claim.Type == "Key").Value;
            this.MembershipNumber = token.Claims.First(claim => claim.Type == "MembershipNumber").Value;
            this.Admin = token.GetBoolClaim("Admin");
            this.Treasurer = token.GetBoolClaim("Treasurer");
            this.CommitteeMember = token.GetBoolClaim("CommitteeMember"); 
            this.Secretary = token.GetBoolClaim("Secretary");
            this.MembershipSecretary = token.GetBoolClaim("MembershipSecretary");
            this.Previewer = token.GetBoolClaim("Previewer");
            this.Developer = token.GetBoolClaim("Developer");
            this.AllowNameToBeUsed = token.GetBoolClaim("AllowNameToBeUsed");
            this.PreferencesLastUpdated = DateTime.Parse(token.Claims.First(claim => claim.Type == "PreferencesLastUpdated").Value);
            this.Name = token.Claims.First(claim => claim.Type == "Name").Value;
            this.Email = token.Claims.First(claim => claim.Type == "Email").Value;
            this.PinResetRequired = token.GetBoolClaim("PinResetRequired");
        }

        public ClaimsIdentity GetIdentity(Member member, string developerName)
        {
            return new ClaimsIdentity(new[]
            {
                new Claim("Key", member.DbKey),
                new Claim("MembershipNumber", member.MembershipNumber.ToString()),
                new Claim("Admin", member.Admin.ToString()),
                new Claim("Treasurer", member.Treasurer.ToString()),
                new Claim("CommitteeMember", member.CommitteeMember.ToString()),
                new Claim("Secretary", member.Secretary.ToString()),
                new Claim("MembershipSecretary", member.MembershipSecretary.ToString()),
                new Claim("Previewer", member.Previewer.ToString()),
                new Claim("Developer", (member.Name == developerName).ToString()),
                new Claim("AllowNameToBeUsed", member.AllowNameToBeUsed.ToString()),
                new Claim("PreferencesLastUpdated", member.PreferencesLastUpdated.ToString("u")),
                new Claim("Name", member.AllowNameToBeUsed ? member.Name : "Anonymous"),
                new Claim("Email", member.Email),
                new Claim("PinResetRequired", member.PinResetRequired.ToString())
            });
        }


    }
}
