using AnglingClubShared.DTOs;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using System.Reflection;

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

        public AboutDto AboutInfo { get; set; } = new AboutDto();
        public bool IsDeveloper { get; set; } = false;
        public string ClientBuiltAt
        {
            get;
            set;
        } = "";

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            AboutInfo = await _aboutService.GetAboutInfo() ?? new AboutDto();

            IsDeveloper = CurrentUser.Developer;

            Assembly curAssembly = typeof(Program).Assembly;
            ClientBuiltAt = $"{curAssembly.GetCustomAttributes(false).OfType<AssemblyTitleAttribute>().FirstOrDefault()!.Title}";

        }

        #endregion Events

        #region Helper Methods


        #endregion Helper Methods
    }
}
