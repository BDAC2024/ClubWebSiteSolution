using AnglingClubShared;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Data.Common;

namespace AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating
{

    public partial class EmbeddedLayoutViewModel : ViewModelBase, 
        IRecipient<BrowserChange>,
        IRecipient<ShowProgress>,
        IRecipient<HideProgress>
    {

        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;
        private readonly BrowserService _browserService;

        private const bool ShowDebugMessages = false;

        public EmbeddedLayoutViewModel(
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IMessenger messenger,
            BrowserService browserService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _messenger = messenger;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this); 
            messenger.Register<ShowProgress>(this); 
            messenger.Register<HideProgress>(this);

            setBrowserDetails();
        }

        [ObservableProperty]
        private DeviceSize _browserSize = DeviceSize.Unknown;

        [ObservableProperty]
        private bool _browserPortrait = false;

        [ObservableProperty]
        private int _browserWidth = 0;

        [ObservableProperty]
        private int _browserHeight = 0;

        [ObservableProperty]
        private bool _showProgressBar = false;

        #region Message Handlers

        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
        }

        public void Receive(HideProgress message)
        {
            ShowProgressBar = false;
        }

        public void Receive(ShowProgress message)
        {
            ShowProgressBar = true;

            ShowConsoleMessage($"ShowProgress: size = {BrowserSize}");
        }

        #endregion Message Handlers

        public void ShowConsoleMessage(string message)
        {
            if (ShowDebugMessages)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");
            }
        }

        private void setBrowserDetails()
        {
            BrowserPortrait = _browserService.IsPortrait;
            BrowserSize = _browserService.DeviceSize;
            BrowserWidth = _browserService.Dimensions.Width;
            BrowserHeight = _browserService.Dimensions.Height;
        }
    }
}
