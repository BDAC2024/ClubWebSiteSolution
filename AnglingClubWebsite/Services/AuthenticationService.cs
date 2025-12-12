using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using AnglingClubWebsite.Services;
using AnglingClubShared.Models.Auth;
using AnglingClubWebsite;
using AnglingClubWebsite.Authentication;
using AnglingClubWebsite.Extensions;
using AnglingClubShared.Entities;
using AnglingClubShared.DTOs;
using static System.Net.WebRequestMethods;


namespace Fishing.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _factory;
        private readonly AuthenticationStateProvider _stateProvider;
        private ILocalStorageService _localStorageService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ICurrentUserService _currentUserService;

        private const string JWT_KEY = nameof(JWT_KEY);
        private const string REFRESH_KEY = nameof(REFRESH_KEY);
        private const string CONTROLLER = "Members";

        //private string? _jwtCache;

        public event Action<string?>? LoginChange;

        public AuthenticationService(IHttpClientFactory factory, ILocalStorageService localStorageService, AuthenticationStateProvider stateProvider, ILogger<AuthenticationService> logger, ICurrentUserService currentUserService)
        {
            _factory = factory;
            _localStorageService = localStorageService;
            _stateProvider = stateProvider;
            _logger = logger;
            _currentUserService = currentUserService;
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

            await customAuthStateProvider.UpdateAuthenticationState(null);

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
            var authenticatedMember = await _localStorageService.ReadEncryptedItem<AuthenticateResponse>(Constants.AUTH_KEY);
            if (authenticatedMember == null)
            {
                return new ClientMemberDto();
            }

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

            return !string.IsNullOrEmpty(jwt);

        }

        public async Task<bool> LoginAsync(AuthenticateRequest model)
        {
            var http = _factory.CreateClient(Constants.HTTP_CLIENT_KEY);
            http.BaseAddress = new Uri($"{http.BaseAddress!.ToString()}api/{CONTROLLER}/");

            _logger.LogInformation($"Accessing {http.BaseAddress}{Constants.API_AUTHENTICATE}");

            var response = await http.PostAsync(Constants.API_AUTHENTICATE,
                                                        JsonContent.Create(model));

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            try
            {
                var content = await response.Content.ReadFromJsonAsync<AuthenticateResponse>();

                if (content == null)
                    throw new InvalidDataException();

                var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;
                await customAuthStateProvider.UpdateAuthenticationState(content);


                //await _localStorageService.SetItemAsync(JWT_KEY, content.Token);
                //await _localStorageService.SetItemAsync(REFRESH_KEY, content.RefreshToken);

                LoginChange?.Invoke(GetUsername(content.Token));

                //_currentUserService.User = await GetCurrentUser();

                return true;

            }
            catch (Exception)
            {

                throw;
            }
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

                    if (!response.IsSuccessStatusCode)
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
    }
}
