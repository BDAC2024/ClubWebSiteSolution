using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;
using AnglingClubWebsite.Authentication;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;


namespace Fishing.Client.Services
{
    public class AuthenticationService : DataServiceBase, IAuthenticationService
    {
        private readonly IHttpClientFactory _factory;
        private readonly AuthenticationStateProvider _stateProvider;
        private ILocalStorageService _localStorageService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly HostBridge _hostBridge;
        private readonly IAuthTokenStore _authTokenStore;


        private const string JWT_KEY = nameof(JWT_KEY);
        private const string REFRESH_KEY = nameof(REFRESH_KEY);
        private const string CONTROLLER = "Members";

        //private string? _jwtCache;

        public event Action<string?>? LoginChange;

        public AuthenticationService(
            IHttpClientFactory factory,
            ILocalStorageService
            localStorageService,
            AuthenticationStateProvider stateProvider,
            ILogger<AuthenticationService> logger,
            ICurrentUserService currentUserService,
            HostBridge hostBridge, // TODO Ang to Blazor Migration - remove when migration complete
            IAuthTokenStore authTokenStore) : base(factory)
        {
            _factory = factory;
            _localStorageService = localStorageService;
            _stateProvider = stateProvider;
            _logger = logger;
            _currentUserService = currentUserService;
            _hostBridge = hostBridge;

            _hostBridge.AuthUpdated += async (authResponse, rememberMe) =>
            {
                if (authResponse == null)
                {
                    await LogoutAsync();
                }
                else
                {
                    await LoginWithResponseAsync(authResponse, rememberMe);
                }
            };
            _authTokenStore = authTokenStore;
        }

        //public async ValueTask<string> GetJwtAsync()
        //{
        //    if (string.IsNullOrEmpty(_jwtCache))
        //        _jwtCache = await _localStorageService.GetItemAsync<string>(JWT_KEY);

        //    return _jwtCache;
        //}

        public async Task LogoutAsync()
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;

            await customAuthStateProvider.UpdateAuthenticationState(null, false);

            //_currentUserService.User = new ClientMemberDto();

            //var response = await _factory.CreateClient(Constants.HTTP_CLIENT_KEY).DeleteAsync("api/authentication/revoke");

            //await _localStorageService.RemoveItemAsync(JWT_KEY);
            //await _localStorageService.RemoveItemAsync(REFRESH_KEY);

            ////_jwtCache = null;

            //await Console.Out.WriteLineAsync($"Revoke gave response {response.StatusCode}");

            //LoginChange?.Invoke(null);
        }

        public async Task<ClientMemberDto> GetCurrentUser()
        {
            //_logger.LogWarning("AuthenticationService.GetCurrentUser called");

            var authenticatedMember = _authTokenStore.Current;

            if (authenticatedMember == null)
            {
                return new ClientMemberDto();
            }

            //_logger.LogWarning($"AuthenticationService.GetCurrentUser authenticatedMember = {JsonSerializer.Serialize(authenticatedMember)}");

            return new ClientMemberDto(new JwtSecurityTokenHandler().ReadJwtToken(authenticatedMember.Token));
        }

        private static string GetUsername(string token)
        {
            var jwt = new JwtSecurityToken(token);

            return jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        }

        public async Task<bool> isLoggedIn()
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;

            var jwt = await customAuthStateProvider.GetToken();

            return !string.IsNullOrEmpty(jwt) && jwt.ToUpper() != Constants.AUTH_EXPIRED;

        }

        public async Task<bool> LoginAsync(AuthenticateRequest model, bool rememberMe = true)
        {
            var http = _factory.CreateClient(Constants.HTTP_CLIENT_KEY);
            http.BaseAddress = new Uri($"{http.BaseAddress!.ToString()}api/{CONTROLLER}/");

            HttpResponseMessage response;

            response = await http.PostAsync(Constants.API_AUTHENTICATE,
                                                            JsonContent.Create(model));
            var content = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();

            await LoginWithResponseAsync(content!, rememberMe);

            return true;
        }


        public async Task LoginWithResponseAsync(AuthenticateResponse content, bool rememberMe)
        {
            if (content == null)
            {
                throw new InvalidDataException();
            }

            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;
            await customAuthStateProvider.UpdateAuthenticationState(content, rememberMe);

            LoginChange?.Invoke(GetUsername(content.Token));
        }


        /*
                public async Task<bool> RefreshAsync()
                {
                    var model = new RefreshModel
                    {
                        AccessToken = await _localStorageService.GetItemAsync<string>(JWT_KEY),
                        RefreshToken = await _localStorageService.GetItemAsync<string>(REFRESH_KEY)
                    };

                    var response = await _factory.CreateClient(Constants.HTTP_CLIENT_KEY).PostAsync("api/authentication/refresh",
                                                                JsonContent.Create(model));

                    if (!response.isSuccessStatusCode)
                    {
                        await LogoutAsync();

                        return false;
                    }

                    var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (content == null)
                        throw new InvalidDataException();

                    await _localStorageService.SetItemAsync(JWT_KEY, content.JwtToken);
                    await _localStorageService.SetItemAsync(REFRESH_KEY, content.RefreshToken);

                    _jwtCache = content.JwtToken;
                    return true;
                }
        */

        public async Task<bool> PinResetRequest(int membershipNumber)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_PIN_RESET_REQUEST}/{membershipNumber}";

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", "");

            var content = await response.Content.ReadFromJsonAsync<bool>();
            return content;
        }
    }
}

