using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;
using AnglingClubWebsite.Extensions;
using Blazored.LocalStorage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AnglingClubWebsite.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IMessenger _messenger;

        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, IMessenger messenger)
        {
            _localStorageService = localStorageService;
            _messenger = messenger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSession = await _localStorageService.ReadEncryptedItem<AuthenticateResponse>(Constants.AUTH_KEY);

                if (userSession == null)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var expired = userSession.Expiration < DateTime.UtcNow;

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.Name, userSession.Id!),
                    new Claim("Token", userSession.Token!),
                    new Claim(ClaimTypes.Expired, expired.ToString()),
                }, "JwtAuth"));

                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(AuthenticateResponse? userSession)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                    new Claim("Id", userSession.Id!),
                    new Claim("Token", userSession.Token!),
                    //new Claim(ClaimTypes.GivenName, userSession.FirstName!),
                    //new Claim(ClaimTypes.Surname, userSession.LastName!),
                }));

                userSession.Expiration = DateTime.Now.AddSeconds(userSession.ExpiresIn);

                await _localStorageService.SaveItemEncrypted(Constants.AUTH_KEY, userSession);

                _messenger.Send(new LoggedIn(new MemberDto(new JwtSecurityTokenHandler().ReadJwtToken(userSession.Token))));
            }
            else
            {
                claimsPrincipal = _anonymous;

                await _localStorageService.RemoveItemAsync(Constants.AUTH_KEY);

                _messenger.Send(new LoggedIn(null));
            }


            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task<string> GetToken()
        {
            var result = string.Empty;

            try
            {
                var userSession = await _localStorageService.ReadEncryptedItem<AuthenticateResponse>(Constants.AUTH_KEY);

                if (userSession != null && DateTime.UtcNow < userSession.Expiration)
                {
                    result = userSession.Token;
                }
            }
            catch
            {
            }

            return result;
        }
    }
}
