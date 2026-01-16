using AnglingClubShared;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Notifications;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class DialogHost: RazorComponentBase
    {
        private DialogRequest? Current;
        private DialogRequest? _lastToastShown;
        private bool Busy;
        SfToast? ToastObj;

        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDialogQueue _dialogQueue;

        public DialogHost(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IDialogQueue dialogQueue) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _currentUserService = currentUserService;
            _dialogQueue = dialogQueue;
        }


        protected override void OnInitialized()
        {
            _dialogQueue.Changed += OnQueueChanged;
            TryShowNext();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Current is null || Current.Kind != DialogKind.Toast)
                return;

            // Guard against re-showing on subsequent renders
            if (ReferenceEquals(Current, _lastToastShown))
                return;

            _lastToastShown = Current;

            await ShowToastAndAdvanceAsync(Current);
        }

        private async Task ShowToastAndAdvanceAsync(DialogRequest toastRequest)
        {
            if (ToastObj is null)
                return;

            await ToastObj.ShowAsync(new ToastModel
            {
                Content = toastRequest.Message,
                Icon = toastRequest.Severity switch
                {
                    DialogSeverity.Success => "e-circle-check",
                    DialogSeverity.Warn => "e-warning",
                    DialogSeverity.Error => "e-circle-close",
                    _ => "e-circle-info"
                },

                CssClass = toastRequest.Severity switch
                {
                    DialogSeverity.Success => "app-snackbar e-toast-success",
                    DialogSeverity.Warn => "app-snackbar e-toast-warning",
                    DialogSeverity.Error => "app-snackbar e-toast-danger",
                    _ => "app-snackbar e-toast-info"
                },
                Timeout = 2500,       // Set to 0 to Leave on screen for testing/debugging
                //ExtendedTimeout = 0 // Leave on screen for testing/debugging
            });

            // Give the renderer/JS a chance to apply the show operation before advancing.
            await Task.Yield();

            Current = null;
            TryShowNext();
            await InvokeAsync(StateHasChanged);
        }


        private void OnQueueChanged()
        {
            // Runs on a non-UI thread sometimes; marshal to UI
            _ = InvokeAsync(() =>
            {
                TryShowNext();
                StateHasChanged();
            });
        }

        private void TryShowNext()
        {
            if (Busy)
            {
                return;
            }

            if (Current is not null)
            {
                return;
            }

            if (_dialogQueue.TryDequeue(out var next) && next is not null)
            {
                Current = next;
            }
        }

        private async Task ConfirmAsync()
        {
            if (Current is null)
            {
                return;
            }

            Busy = true;

            try
            {
                var handler = Current.OnConfirmAsync;

                StateHasChanged();

                if (handler is not null)
                {
                    await handler();
                }
            }
            finally
            {
                Current = null;
                Busy = false;
                TryShowNext();
                await InvokeAsync(StateHasChanged);
            }
        }

        private Task CancelAsync() => CloseAsync();

        private async Task CloseAsync()
        {
            Current = null;
            TryShowNext();
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            _dialogQueue.Changed -= OnQueueChanged;
        }
    }


}
