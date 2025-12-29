using AnglingClubShared;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite
{
    public class AppViewModel : ViewModelBase
    {
        private readonly BrowserService _browserService;
        private readonly IMessenger _messsenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;

        public AppViewModel(
            BrowserService browserService, 
            IMessenger messsenger, 
            IAuthenticationService authenticationService, 
            ICurrentUserService currentUserService) : base(messsenger, currentUserService, authenticationService)
        {
            _browserService = browserService;
            _messsenger = messsenger;
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
        }

        public async Task SetupBrowserDetails()
        {
            await _browserService.GetDimensions();

            _messsenger.Send(new BrowserChange());
        }
    }
}
