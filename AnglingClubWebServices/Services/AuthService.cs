using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthOptions _authOptions;
        private readonly IMemberRepository _memberRepository;

        private const int MAX_FAILED_LOGINS = 10;
        private const int MINUTES_TO_LOCKOUT = 2;

        public AuthService(IOptions<AuthOptions> opts,
            IMemberRepository memberRepository)
        {
            _authOptions = opts.Value;
            _memberRepository = memberRepository;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var member = (await _memberRepository.GetMembers(EnumUtils.CurrentSeason())).SingleOrDefault(x => x.MembershipNumber == model.MembershipNumber);

            // return null if user not found or PIN invalid
            if (member == null)
            {
                return null;
            }

            // Reject if locked out
            if (member.FailedLoginAttempts > MAX_FAILED_LOGINS && member.LastLoginFailure.AddMinutes(MINUTES_TO_LOCKOUT) > DateTime.Now)
            {
                throw new Exception($"Too many failed login attempts. Your account will be locked for {MINUTES_TO_LOCKOUT} minutes before you can try again.");
            }

            // Only allow a few failed logins before locking for a short time
            if (!member.ValidPin(model.Pin))
            {
                if (member.LastLoginFailure.AddMinutes(MINUTES_TO_LOCKOUT) < DateTime.Now)
                {
                    member.LastLoginFailure = DateTime.Now;
                    member.FailedLoginAttempts = 0;
                }

                member.FailedLoginAttempts++;

                await _memberRepository.AddOrUpdateMember(member);

                return null;
            }

            member.FailedLoginAttempts = 0;

            await _memberRepository.AddOrUpdateMember(member);


            // authentication successful so generate jwt token
            var token = generateJwtToken(member);

            return new AuthenticateResponse(member, token);
        }
        public async Task<Member> GetByKey(string key)
        {
            var member = (await _memberRepository.GetMembers((Season?)EnumUtils.CurrentSeason())).Single(x => x.DbKey == key);
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
                    new Claim("Admin", member.Admin.ToString()),
                    new Claim("AllowNameToBeUsed", member.AllowNameToBeUsed.ToString()),
                    new Claim("PreferencesLastUpdated", member.PreferencesLastUpdated.ToString("u")),
                    new Claim("Name", member.AllowNameToBeUsed ? member.Name : "Anonymous"),
                    new Claim("PinResetRequired", member.PinResetRequired.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.AuthExpireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string HashText(string text, string salt, HashAlgorithm hasher)
        {
            byte[] textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(text, salt));
            byte[] hashedBytes = hasher.ComputeHash(textWithSaltBytes);
            hasher.Clear();
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
