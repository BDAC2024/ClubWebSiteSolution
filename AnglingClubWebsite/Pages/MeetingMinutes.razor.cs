using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
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

        #region Properties

        public bool AddingMinutes = false;

        #endregion Properties

        #region Events

        public async Task AddMinutesHandler()
        {
            AddingMinutes = true;
        }

        #endregion Events
    }
}
