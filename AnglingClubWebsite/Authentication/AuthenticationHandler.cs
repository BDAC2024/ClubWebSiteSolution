using AnglingClubShared.Extensions;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Headers;

namespace AnglingClubWebsite.Authentication
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AuthenticationStateProvider _stateProvider;
        private readonly IConfiguration _configuration;
        private readonly IMessenger _messenger;
        private readonly IDialogQueue _dialogQueue;

        private bool _refreshing = false;

        public AuthenticationHandler(
            IAuthenticationService authenticationService,
            IConfiguration configuration,
            AuthenticationStateProvider stateProvider,
            IMessenger messenger,
            IDialogQueue dialogQueue)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            _stateProvider = stateProvider;
            _messenger = messenger;
            _dialogQueue = dialogQueue;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var customAuthStateProvider = (CustomAuthenticationStateProvider)_stateProvider;

            var jwt = await customAuthStateProvider.GetToken();

            //if (jwt == Constants.AUTH_EXPIRED)
            //{
            //    throw new UserSessionExpiredException();
            //}

            //Console.WriteLine($"Checking:{request.RequestUri?.AbsoluteUri}");
            //Console.WriteLine($"... to see if it starts with: {_configuration[Constants.API_ROOT_KEY]}");

            var isToServer = request.RequestUri?.AbsoluteUri.StartsWith(_configuration[Constants.API_ROOT_KEY] ?? "") ?? false;

            var isToDevTunnel = request.RequestUri?.AbsoluteUri.Contains(_configuration["uks1.devtunnels.ms"] ?? "") ?? false;

            //Console.WriteLine($"... result: {isToServer}");

            //Console.WriteLine($"Is jwt NOT null : {!string.IsNullOrEmpty(jwt)}");

            if ((isToServer || isToDevTunnel) && !string.IsNullOrEmpty(jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }

            //Console.WriteLine($"Therefore auth is: {request.Headers.Authorization}");

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (!_refreshing && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Handle the "Require re-login" setting
                    var user = await _authenticationService.GetCurrentUser();
                    if (!user.Id.IsNullOrEmpty())
                    {
                        // Must be a forced re-login
                        _dialogQueue.Enqueue(new DialogRequest
                        {
                            Kind = DialogKind.Confirm,
                            Severity = DialogSeverity.Info,
                            Title = "Re-login required",
                            Message = $"You have now been logged out. System changes require that you login again.",
                            CancelText = "",
                            ConfirmText = "OK",
                            OnConfirmAsync = async () =>
                            {
                                await customAuthStateProvider.UpdateAuthenticationState(null, false, true);
                            }
                        });
                    }

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
            catch (ApiUnauthorizedException ex)
            {
                // authError will be null or empty if a standard 401. If it has a value, it indicates a specific auth error message from the server (usually expired).
                string? authError = ex.Problem?.ExtensionData?["authError"].GetString();

                if (!string.IsNullOrEmpty(authError))
                {
                    string title = "Unexpected error";
                    var message = "Please login again.";

                    if (await _authenticationService.sessionExpired())
                    {
                        title = "Login expired";
                        message = "Your login session has expired. Please login again.";
                    }
                    else
                    {
                        // Handle the "Require re-login" setting
                        var user = await _authenticationService.GetCurrentUser();
                        if (!user.Id.IsNullOrEmpty())
                        {
                            // Must be a forced re-login
                            title = "Re-login required";
                            message = "You have now been logged out. System changes require that you login again.";
                        }
                    }
                    _dialogQueue.Enqueue(new DialogRequest
                    {
                        Kind = DialogKind.Confirm,
                        Severity = DialogSeverity.Info,
                        Title = title,
                        Message = message,
                        CancelText = "",
                        ConfirmText = "OK",
                        OnConfirmAsync = async () =>
                        {
                            await customAuthStateProvider.UpdateAuthenticationState(null, false, true);
                        }
                    });

                }
                return new HttpResponseMessage();
            }

        }
    }

}
