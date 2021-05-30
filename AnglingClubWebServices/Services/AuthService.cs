using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthOptions _authOptions;
        private readonly IMemberRepository _memberRepository;

        public AuthService(IOptions<AuthOptions> opts,
            IMemberRepository memberRepository)
        {
            _authOptions = opts.Value;
            _memberRepository = memberRepository;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var member = (await _memberRepository.GetMembers()).SingleOrDefault(x => x.MembershipNumber == model.MembershipNumber && x.Pin == model.Pin);

            // return null if user not found
            if (member == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(member);

            return new AuthenticateResponse(member, token);
        }
        public async Task<Member> GetByKey(string key)
        {
            var member = (await _memberRepository.GetMembers()).Single(x => x.DbKey == key);
            return member;
        }



        private string generateJwtToken(Member member)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authOptions.AuthSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("Key", member.DbKey), 
                    new Claim("MembershipNumber", member.MembershipNumber.ToString()),
                    new Claim("IsAdmin", member.Admin.ToString()),
                    new Claim("AllowNameToBeUsed", member.AllowNameToBeUsed.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.AuthExpireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
