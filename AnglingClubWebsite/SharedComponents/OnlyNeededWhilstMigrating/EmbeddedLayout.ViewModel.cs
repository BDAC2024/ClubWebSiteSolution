using AnglingClubShared;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating
{

    public partial class EmbeddedLayoutViewModel : ViewModelBase,
        IRecipient<BrowserChange>
    {

        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMessenger _messenger;
        public readonly BrowserService BrowserService;
        private readonly IGlobalService _globalService;

        private const bool ShowDebugMessages = false;

        public EmbeddedLayoutViewModel(
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IMessenger messenger,
            BrowserService browserService,
            IGlobalService globalService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _messenger = messenger;
            BrowserService = browserService;

            messenger.Register<BrowserChange>(this);

            setBrowserDetails();
            _globalService = globalService;

            _globalService.IsEmbedded = true;
        }

        [ObservableProperty]
        private DeviceSize _browserSize = DeviceSize.Unknown;

        [ObservableProperty]
        private bool _browserPortrait = false;

        [ObservableProperty]
        private int _browserWidth = 0;

        [ObservableProperty]
        private int _browserHeight = 0;

        #region Message Handlers

        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
        }

        #endregion Message Handlers

        public void ShowConsoleMessage(string message)
        {
            if (ShowDebugMessages)
            {
#pragma warning disable CS0162 // Unreachable code detected
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        public void setBrowserDetails()
        {
            BrowserPortrait = BrowserService.IsPortrait;
            BrowserSize = BrowserService.DeviceSize;
            BrowserWidth = BrowserService.Dimensions.Width;
            BrowserHeight = BrowserService.Dimensions.Height;
        }
    }
}
