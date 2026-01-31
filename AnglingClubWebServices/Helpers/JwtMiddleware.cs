using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Helpers
{
    public class JwtMiddleware
    {
        // Note: From https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api

        private readonly RequestDelegate _next;
        private readonly AuthOptions _authOptions;

        public JwtMiddleware(RequestDelegate next,
            IOptions<AuthOptions> opts)
        {
            _next = next;
            _authOptions = opts.Value;
        }

        public async Task Invoke(HttpContext context, IAuthService memberService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await attachUserToContext(context, memberService, token);
            }

            await _next(context);
        }

        private async Task attachUserToContext(HttpContext context, IAuthService memberService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_authOptions.AuthSecretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userKey = jwtToken.Claims.First(x => x.Type == "Key").Value;
                if (string.IsNullOrWhiteSpace(userKey))
                {
                    context.Items["AuthError"] = "invalid_token";
                    return;
                }

                // attach user to context on successful jwt validation
                context.Items["User"] = await memberService.GetAuthorisedUserByKey(userKey);
            }
            catch (SecurityTokenExpiredException)
            {
                context.Items["AuthError"] = "expired_token";
            }
            catch (SecurityTokenException)
            {
                context.Items["AuthError"] = "invalid_token";
            }
            catch (Exception ex)
            {
                // unexpected server-side failure while resolving user
                context.Items["AuthError"] = "auth_failure";
                context.Items["AuthErrorDetail"] = ex.Message; // log it, don't return it
            }
        }
    }
}
