using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor;

namespace AnglingClubWebsite
{
    public partial class App : ComponentBase,
        IRecipient<TurnOnDebugMessages>,
        IRecipient<ShowConsoleMessage>,
        IRecipient<ShowMessage>,
        IRecipient<ShowToast>
    {
        private readonly BrowserService _browserService;
        private readonly IDialogQueue _dialogQueue;
        private readonly HostBridge _hostBridge;
        private readonly IMessenger _messenger;
        private readonly NavigationManager _navigationManager;
        private readonly ICurrentUserService _currentUserService;

        private ErrorBoundary? _errorBoundary;
        private bool _errorReported;
        private bool _showDebugMessages;

        public App(
            BrowserService browserService,
            IDialogQueue dialogQueue,
            HostBridge hostBridge,
            IMessenger messenger,
            NavigationManager navigationManager,
            ICurrentUserService currentUserService)
        {
            _browserService = browserService;
            _dialogQueue = dialogQueue;
            _hostBridge = hostBridge;
            _messenger = messenger;
            _navigationManager = navigationManager;
            _currentUserService = currentUserService;

            messenger.Register<TurnOnDebugMessages>(this);
            messenger.Register<ShowConsoleMessage>(this);
            messenger.Register<ShowMessage>(this);
            messenger.Register<ShowToast>(this);
        }

        private string ActiveBreakpoint { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            SfMediaQuery.Small.MediaQuery = "(max-width: 768px)";
            SfMediaQuery.Medium.MediaQuery = "(min-width: 768px)";
            SfMediaQuery.Large.MediaQuery = "(min-width: 1280px)";
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await _hostBridge.InitializeAsync(); // TODO Ang to Blazor Migration - only needed until migration complete
        }

        public async Task OnBreakpointChanged(BreakpointChangedEventArgs args)
        {
            await _browserService.GetDimensions();
            _messenger.Send(new BrowserChange());
        }

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

        public void Receive(ShowToast message)
        {
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Toast,
                Severity = message.State.GetDialogSeverity(),
                Message = message.Message,
            });
        }

        public void Receive(TurnOnDebugMessages message)
        {
            _showDebugMessages = message.YesOrNo;
        }

        public void Receive(ShowConsoleMessage message)
        {
            ShowConsoleMessage(message.Content, message.showAlways);
        }

        private async Task ReportUiErrorOnce(Exception ex)
        {
            if (_errorReported)
            {
                return;
            }

            _errorReported = true;
            ReportUiError(ex);

            // Give the UI a tick to render the popup before recovery.
            await Task.Yield();

            // Reset the boundary so the app is usable again.
            _errorBoundary?.Recover();
        }

        private void ReportUiError(Exception ex)
        {
            // TODO: replace with logger/toast/telemetry
            Console.Error.WriteLine(ex);
            _messenger.Send(new ShowMessage(
                MessageState.Error,
                "Something went wrong",
                "Please try again. If the problem persists, contact a committee member."));
        }

        private void ShowConsoleMessage(string message, bool showAlways = false)
        {
            if (_showDebugMessages || showAlways)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
            }

            if (_currentUserService.User.Developer)
            {
                Console.WriteLine($"DEV: {DateTime.Now:HH:mm:ss} - {message}");
            }
        }

        // TODO Ang to Blazor Migration - only needed until migration complete
        private Type GetDefaultLayout()
        {
            var uri = new Uri(_navigationManager.Uri);
            var query = uri.Query ?? string.Empty;

            // Use EmbeddedLayout when called from Angular iframe: /new/...?...embedded=true
            return query.Contains("embedded=true", StringComparison.OrdinalIgnoreCase)
                ? typeof(EmbeddedLayout)
                : typeof(MainLayout);
        }
    }
}
