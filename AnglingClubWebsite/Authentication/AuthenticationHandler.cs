using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Components.Authorization;
using AnglingClubWebsite.Services;
using AnglingClubShared;
using CommunityToolkit.Mvvm.Messaging;
using AnglingClubShared.Enums;
using AnglingClubShared.Exceptions;

namespace AnglingClubWebsite.Authentication
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticationStateProvider _stateProvider;
        private readonly IConfiguration _configuration;
        private readonly AnonymousRoutes _anonymousRoutes;
        private readonly IMessenger _messenger;
        private readonly IAppDialogService _appDialogService;

        private bool _refreshing = false;

        public AuthenticationHandler(
            IAuthenticationService authenticationService,
            IConfiguration configuration,
            AuthenticationStateProvider stateProvider,
            AnonymousRoutes anonymousRoutes,
            IMessenger messenger,
            IAppDialogService appDialogService)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            _stateProvider = stateProvider;
            _anonymousRoutes = anonymousRoutes;
            _messenger = messenger;
            _appDialogService = appDialogService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;
            var isAnonymous = _anonymousRoutes.Contains(request);

            var jwt = await customAuthStateProvider.GetToken();

            if (string.IsNullOrEmpty(jwt) && !isAnonymous)
            {
                throw new UserSessionExpiredException();
            }

            //Console.WriteLine($"Checking:{request.RequestUri?.AbsoluteUri}");
            //Console.WriteLine($"... to see if it starts with: {_configuration[Constants.API_ROOT_KEY]}");

            var isToServer = request.RequestUri?.AbsoluteUri.StartsWith(_configuration[Constants.API_ROOT_KEY] ?? "") ?? false;

            //Console.WriteLine($"... result: {isToServer}");

            //Console.WriteLine($"Is jwt NOT null : {!string.IsNullOrEmpty(jwt)}");

            if (isToServer && !string.IsNullOrEmpty(jwt))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            //Console.WriteLine($"Therefore auth is: {request.Headers.Authorization}");

            var response = await base.SendAsync(request, cancellationToken);

            if (!_refreshing && response.StatusCode == HttpStatusCode.Unauthorized)
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

    }
}
