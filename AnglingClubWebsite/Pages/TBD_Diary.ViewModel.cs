using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class DiaryViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public DiaryViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService) : base(messenger, currentUserService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }
    }
}
