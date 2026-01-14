using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite
{
    public partial class AppViewModel : ViewModelBase,
        IRecipient<TurnOnDebugMessages>,
        IRecipient<ShowConsoleMessage>,
        IRecipient<ShowMessage>
    {
        private readonly BrowserService _browserService;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRefDataService _refDataService;
        private readonly IDialogQueue _dialogQueue;

        public AppViewModel(
            BrowserService browserService,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IRefDataService refDataService,
            IDialogQueue dialogQueue) : base(messenger, currentUserService, authenticationService)
        {
            _browserService = browserService;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _refDataService = refDataService;

            messenger.Register<TurnOnDebugMessages>(this);
            messenger.Register<ShowConsoleMessage>(this);
            messenger.Register<ShowMessage>(this);

            _dialogQueue = dialogQueue;
        }

        #region Properties

        [ObservableProperty]
        private bool _showDebugMessages = true;

        #endregion Properties

        public async Task SetupBrowserDetails()
        {
            await _browserService.GetDimensions();

            _messenger.Send(new BrowserChange());
        }

        #region Messaging

        public void Receive(ShowMessage message)
        {
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Alert,
                Severity = message.State.GetDialogSeverity(),
                Title = message.Title,
                Message = message.Body,
            });

        }

        public void Receive(TurnOnDebugMessages message)
        {
            ShowDebugMessages = message.YesOrNo;
        }

        public void Receive(ShowConsoleMessage message)
        {
            ShowConsoleMessage(message.Content);
        }

        #endregion Messaging

        #region Helpers

        public void ShowConsoleMessage(string message)
        {
            if (ShowDebugMessages)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");
            }
        }

        #endregion Helpers
    }
}
