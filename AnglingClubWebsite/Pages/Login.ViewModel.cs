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
using System.ComponentModel.DataAnnotations;

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
            IAppDialogService appDialogService,
            ICurrentUserService currentUserService) : base(messenger, currentUserService)
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
                        _messenger.Send<SelectMenuItem>(new SelectMenuItem(Caller ?? "/"));
                        _messenger.Send<HideProgress>();
                    }
                    else
                    {
                        Submitting = false;
                        _appDialogService.SendMessage(MessageState.Warn, "Sign In Failed", "Invalid username or password");
                        _messenger.Send<HideProgress>();
                    }

                }
                catch (Exception ex)
                {
                    Submitting = false;
                    //_logger.LogError(ex, $"Login failed: {ex.Message}");
                    _messenger.Send<ShowConsoleMessage>(new ShowConsoleMessage($"Login failed: {ex.Message}"));
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

        public partial class LoginDetails : ObservableValidator
        {
            [Required]
            [MinLength(1)]
            [NotifyDataErrorInfo]
            [ObservableProperty]
            private string _membershipNumber;

            //public string MembershipNumber
            //{
            //    get => _membershipNumber;
            //    set => SetProperty(ref _membershipNumber, value, true);
            //}

            [Required]
            [MinLength(1)]
            [NotifyDataErrorInfo]
            [ObservableProperty]
            private string _pin;

            //public string Pin
            //{
            //    get => _pin;
            //    set => SetProperty(ref _pin, value, true);
            //}

            public void Validate() => ValidateAllProperties();
        }
    }


}
