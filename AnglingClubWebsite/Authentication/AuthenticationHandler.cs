using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using AnglingClubWebsite.Services;

namespace AnglingClubWebsite.Authentication
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticationStateProvider _stateProvider;
        private readonly IConfiguration _configuration;
        private readonly AnonymousRoutes _anonymousRoutes;
        private bool _refreshing;

        public AuthenticationHandler(IAuthenticationService authenticationService, IConfiguration configuration, AuthenticationStateProvider stateProvider, AnonymousRoutes anonymousRoutes)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            _stateProvider = stateProvider;
            _anonymousRoutes = anonymousRoutes;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;

            if (request.RequestUri!.ToString().EndsWith("/authenticate") ||
                _anonymousRoutes.Contains(request.RequestUri) || 
                await _authenticationService.isLoggedIn())
            {
                var jwt = await customAuthStateProvider.GetToken();

                var isToServer = request.RequestUri?.AbsoluteUri.StartsWith(_configuration[Constants.API_ROOT_KEY] ?? "") ?? false;

                if (isToServer && !string.IsNullOrEmpty(jwt))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                var response = await base.SendAsync(request, cancellationToken);

                if (!_refreshing && !string.IsNullOrEmpty(jwt) && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        _refreshing = true;

                        throw new UnauthorizedAccessException("Login failed.");
                        //if (await _authenticationService.RefreshAsync())
                        //{
                        //    jwt = await customAuthStateProvider.GetToken();

                        //    if (isToServer && !string.IsNullOrEmpty(jwt))
                        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                        //    response = await base.SendAsync(request, cancellationToken);
                        //}
                    }
                    finally
                    {
                        _refreshing = false;
                    }
                }
                return response;
            }
            else
            {
                throw new UnauthorizedAccessException("Login failed.");
            }
        }

    }
}
