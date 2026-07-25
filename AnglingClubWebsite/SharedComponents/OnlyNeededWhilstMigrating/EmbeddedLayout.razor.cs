using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating
{
    public partial class EmbeddedLayout : LayoutComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly BrowserService _browserService;

        public EmbeddedLayout(
            IMessenger messenger,
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            BrowserService browserService,
            IGlobalService globalService)
        {
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);

            SetBrowserDetails();
            globalService.IsEmbedded = true;
        }

        public bool BrowserPortrait { get; set; }
        public DeviceSize BrowserSize { get; set; } = DeviceSize.Unknown;
        public int BrowserWidth { get; set; }
        public int BrowserHeight { get; set; }

        // TODO Ang to Blazor Migration - this file only needed whilst migrating from Angular to Blazor.
        public bool debuggingDeviceType = false;

        protected override async Task OnInitializedAsync()
        {
            _currentUserService.User = await _authenticationService.GetCurrentUser();
        }

        public void Receive(BrowserChange message)
        {
            SetBrowserDetails();
            _ = InvokeAsync(StateHasChanged);
        }

        private void SetBrowserDetails()
        {
            BrowserPortrait = _browserService.IsPortrait;
            BrowserSize = _browserService.DeviceSize;
            BrowserWidth = _browserService.Dimensions.Width;
            BrowserHeight = _browserService.Dimensions.Height;
        }
    }
}
