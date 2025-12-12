using AnglingClubShared.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnglingClubShared.DTOs
{
    public class ClientMemberDto
    {
        public string Id { get; set; }
        public string MembershipNumber { get; set; } = "";
        public bool Admin { get; set; } = false;
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
            this.Admin = bool.Parse(token.Claims.First(claim => claim.Type == "Admin").Value);
            this.Developer = bool.Parse(token.Claims.First(claim => claim.Type == "Developer").Value);
            this.AllowNameToBeUsed = bool.Parse(token.Claims.First(claim => claim.Type == "AllowNameToBeUsed").Value);
            this.PreferencesLastUpdated = DateTime.Parse(token.Claims.First(claim => claim.Type == "PreferencesLastUpdated").Value);
            this.Name = token.Claims.First(claim => claim.Type == "Name").Value;
            this.Email = token.Claims.First(claim => claim.Type == "Email").Value;
            this.PinResetRequired = bool.Parse(token.Claims.First(claim => claim.Type == "PinResetRequired").Value);
        }

        public ClaimsIdentity GetIdentity(Member member, string developerName)
        {
            return new ClaimsIdentity(new[]
            {
                new Claim("Key", member.DbKey),
                new Claim("MembershipNumber", member.MembershipNumber.ToString()),
                new Claim("Admin", member.Admin.ToString()),
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
