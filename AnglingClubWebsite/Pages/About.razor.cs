using AnglingClubShared.DTOs;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class About
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly IAboutService _aboutService;

        public About(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService,
                        IMessenger messenger,
                        IAboutService aboutService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _aboutService = aboutService;
        }

        #region Properties

        public AboutDto AboutInfo { get; set; }
        public bool IsDeveloper { get; set; } = false;

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            AboutInfo = await _aboutService.GetAboutInfo() ?? new AboutDto();

            IsDeveloper = CurrentUser.Developer;
        }

        #endregion Events

        #region Helper Methods


        #endregion Helper Methods
    }
}
