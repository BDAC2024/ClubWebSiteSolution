using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class WatersViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;

        public WatersViewModel(
            IMessenger messenger, 
            ICurrentUserService currentUserService, 
            IAuthenticationService authenticationService) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
        }
    }
}
