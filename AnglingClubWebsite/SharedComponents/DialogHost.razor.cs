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
        private bool Busy;

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
