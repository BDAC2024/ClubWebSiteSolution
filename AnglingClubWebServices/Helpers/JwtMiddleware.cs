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
                attachUserToContext(context, memberService, token);
            }

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, IAuthService memberService, string token)
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
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userKey = jwtToken.Claims.First(x => x.Type == "Key").Value;


                try
                {
                    // attach user to context on successful jwt validation
                    context.Items["User"] = memberService.GetAuthorisedUserByKey(userKey).Result;
                }
                catch (Exception ex)
                {
                    context.Items["UserError"] = ex.Message;
                    throw;
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
