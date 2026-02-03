using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;
using AnglingClubWebsite.Models;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
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
        private readonly HostBridge _hostBridge;
        private readonly IJSRuntime _js;

        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private bool _isEmbedded = true;

        public CustomAuthenticationStateProvider(
            ILocalStorageService localStorageService,
            IMessenger messenger,
            ILogger<CustomAuthenticationStateProvider> logger,
            ISessionStorageService sessionStorageService,
            IAuthTokenStore authTokenStore,
            HostBridge hostBridge,
            IJSRuntime js)
        {
            _localStorageService = localStorageService;
            _messenger = messenger;
            _logger = logger;
            _sessionStorageService = sessionStorageService;
            _authTokenStore = authTokenStore;
            _hostBridge = hostBridge;
            _js = js;
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

        public async Task UpdateAuthenticationState(AuthenticateResponse? userSession, bool rememberMe, bool requestHostLogout = false)
        {
            var userSessionAsString = JsonSerializer.Serialize(userSession, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            //_logger.LogWarning($"[UpdateAuthenticationState] called with userSession = {userSessionAsString} and rememberMe = {rememberMe}");

            // TODO Ang to Blazor Migration - only needed until migration is complete
            _isEmbedded = await _hostBridge.IsEmbeddedAsync();

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
                    //Console.Write("Token being saved to \"Token being saved to SessionStorage\"); - via console");
                    //_logger.LogInformation($"Token being saved to SessionStorage - via log - [{userSessionAsString}]");
                    await _sessionStorageService.SetItemAsStringAsync(Constants.AUTH_KEY, userSessionAsString); // TODO Ang to Blazor Migration - remove after migration
                    //await _sessionStorageService.SaveItemEncrypted(Constants.AUTH_KEY, userSession); // TODO Ang to Blazor Migration - re-instate after migration
                }

                _authTokenStore.Current = userSession;

                // TODO Ang to Blazor Migration - only needed until migration is complete
                if (!_isEmbedded)
                {
                    _messenger.Send(new LoggedIn(new ClientMemberDto(new JwtSecurityTokenHandler().ReadJwtToken(userSession.Token))));  // TODO Ang to Blazor Migration - keep just this once migration is complete
                }

            }
            else
            {
                claimsPrincipal = _anonymous;

                await _localStorageService.RemoveItemAsync(Constants.AUTH_KEY);
                await _sessionStorageService.RemoveItemAsync(Constants.AUTH_KEY);

                _authTokenStore.Current = null;


                // TODO Ang to Blazor Migration - only needed until migration is complete
                if (!_isEmbedded)
                {
                    var anonUser = new LoggedIn(new ClientMemberDto(), true);
                    _messenger.Send(anonUser); // TODO Ang to Blazor Migration - keep just this once migration is complete
                }
                else
                {
                    if (requestHostLogout)
                    {

                        // In embedded mode, we need to notify the host that the user has logged out and show login page
                        await _js.InvokeVoidAsync("blazorHostBridge.requestLogoutShowLogin");

                    }
                }
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
                        return Constants.AUTH_EXPIRED;
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
