using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;
using AnglingClubWebsite.Extensions;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AnglingClubWebsite.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IMessenger _messenger;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;
        private readonly IAuthTokenStore _authTokenStore;

        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(
            ILocalStorageService localStorageService,
            IMessenger messenger,
            ILogger<CustomAuthenticationStateProvider> logger,
            ISessionStorageService sessionStorageService,
            IAuthTokenStore authTokenStore)
        {
            _localStorageService = localStorageService;
            _messenger = messenger;
            _logger = logger;
            _sessionStorageService = sessionStorageService;
            _authTokenStore = authTokenStore;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //_logger.LogWarning($"[GetAuthenticationStateAsync] called with");
            try
            {
                
                var userSession = _authTokenStore.Current;

                if (userSession == null)
                {
                    return new AuthenticationState(_anonymous);
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

        public async Task UpdateAuthenticationState(AuthenticateResponse? userSession, bool rememberMe)
        {
            var userSessionAsString = JsonSerializer.Serialize(userSession, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            //_logger.LogWarning($"[UpdateAuthenticationState] called with userSession = {userSessionAsString} and rememberMe = {rememberMe}");

            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                    new Claim("Id", userSession.Id!),
                    new Claim("Token", userSession.Token!),
                    //new Claim(ClaimTypes.GivenName, userSession.FirstName!),
                    //new Claim(ClaimTypes.Surname, userSession.LastName!),
                }, "JwtAuth"));
                
                userSession.Expiration = DateTime.UtcNow.AddSeconds(userSession.ExpiresIn);

                if (rememberMe)
                {
                    
                    await _localStorageService.SetItemAsStringAsync(Constants.AUTH_KEY, userSessionAsString); // TODO Ang to Blazor Migration - remove after migration
                    //await _localStorageService.SaveItemEncrypted(Constants.AUTH_KEY, userSession); // TODO Ang to Blazor Migration - re-instate after migration
                }
                else
                {
                    await _sessionStorageService.SetItemAsStringAsync(Constants.AUTH_KEY, userSessionAsString); // TODO Ang to Blazor Migration - remove after migration
                    //await _sessionStorageService.SaveItemEncrypted(Constants.AUTH_KEY, userSession); // TODO Ang to Blazor Migration - re-instate after migration
                }

                _authTokenStore.Current = userSession;

                // TODO Ang to Blazor Migration - put the following back once migration is complete
                //_messenger.Send(new LoggedIn(new ClientMemberDto(new JwtSecurityTokenHandler().ReadJwtToken(userSession.Token))));
            }
            else
            {
                claimsPrincipal = _anonymous;

                await _localStorageService.RemoveItemAsync(Constants.AUTH_KEY);
                await _sessionStorageService.RemoveItemAsync(Constants.AUTH_KEY);

                _authTokenStore.Current = null;

                var anonUser = new LoggedIn(new ClientMemberDto());

                // TODO Ang to Blazor Migration - put the following back once migration is complete
                //_messenger.Send(anonUser);
            }

            //_logger.LogWarning($"[UpdateAuthenticationState] called, now calling NotifyAuthenticationStateChanged");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task<string> GetToken()
        {
            await Task.Delay(0);

            var result = string.Empty;

            try
            {
                var userSession = _authTokenStore.Current;

                if (userSession != null)
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(userSession.Token);

                    var expiry = jwt.ValidTo;

                    if (DateTime.UtcNow < expiry)
                    {
                        result = userSession.Token;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetToken: {ex.Message}");
                throw;
            }

            return result;
        }
    }
}
