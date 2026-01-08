using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class MeetingMinutes
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public MeetingMinutes(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService, 
                        IMessenger messenger) : base (messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }
    }
}
