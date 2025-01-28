using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class NewsCardViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public NewsCardViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService) : base(messenger, currentUserService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

    }
}
