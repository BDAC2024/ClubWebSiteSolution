using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Notifications;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Reflection;
using AnglingClubWebsite.SharedComponents;
using AnglingClubWebsite.Services;
using AnglingClubShared.Models.Auth;
using AnglingClubShared;
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Pages
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly ILogger<LoginViewModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        private readonly IAppDialogService _appDialogService;

        public LoginViewModel(
            ILogger<LoginViewModel> logger,
            IHttpClientFactory httpClientFactory,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            INavigationService navigationService,
            IAppDialogService appDialogService) : base(messenger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            _appDialogService = appDialogService;
        }

        [ObservableProperty]
        //[NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _submitting = false;

        [ObservableProperty]
        private LoginDetails _loginInfo = new LoginDetails();

        [ObservableProperty]
        private AuthenticateRequest _loginModel = new AuthenticateRequest();

        [ObservableProperty]
        private string _caller = "/";

        [RelayCommand(CanExecute = nameof(CanWeLogin))]
        private async Task Login()
        {
            if (int.TryParse(LoginInfo.MembershipNumber, out var membershipNo))
            {
                LoginModel.MembershipNumber = membershipNo;
            }
            if (int.TryParse(LoginInfo.Pin, out var pinNo))
            {
                LoginModel.Pin = pinNo;
            }

            LoginModel.Validate();

            if (!LoginModel.HasErrors)
            {
                _messenger.Send<ShowProgress>();

                Submitting = true;

                try
                {
                    if (await _authenticationService.LoginAsync(LoginModel))
                    {
                        _navigationService.NavigateTo(Caller ?? "/", true);
                        _messenger.Send<HideProgress>();
                    }
                    else
                    {
                        _appDialogService.SendMessage(MessageState.Warn, "Sign In Failed", "Invalid username or password");
                        _messenger.Send<HideProgress>();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Login failed: {ex.Message}");

                    _appDialogService.SendMessage(MessageState.Error, "Sign In Failed", "An unexpected error occurred");

                }
                finally
                {
                    Submitting = false;
                    _messenger.Send<HideProgress>();
                }
            }
        }

        private bool CanWeLogin()
        {
            var valid = !(LoginModel.HasErrors || Submitting);
            return valid;
        }

        public class LoginDetails
        {
            public string MembershipNumber { get; set; } = "";
            public string Pin { get; set; } = "";
        }
    }


}
