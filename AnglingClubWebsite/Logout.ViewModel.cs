using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite
{
    public partial class LogoutViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

        public LogoutViewModel(IMessenger messenger, INavigationService navigationService, IAuthenticationService authenticationService) : base(messenger)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
        }

        public override async Task Loaded()
        {
            await base.Loaded();
            await _authenticationService.LogoutAsync();
        }

    }
}
