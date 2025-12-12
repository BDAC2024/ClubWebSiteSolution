using AnglingClubShared;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class LogoutViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        public LogoutViewModel(
            IMessenger messenger, 
            INavigationService navigationService, 
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService) : base(messenger, currentUserService, authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

        public override async Task Loaded()
        {
            await base.Loaded();
            await _authenticationService.LogoutAsync();
            NavToPage("/");
        }

    }
}
