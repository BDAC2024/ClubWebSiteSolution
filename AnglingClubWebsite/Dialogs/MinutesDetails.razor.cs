using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.Dialogs
{
    public partial class MinutesDetails
    {
        [Parameter] required public bool Visible { get; set; } = false;
        /// <summary>
        /// This is a name-based convention that will trigger a 2-way binding.
        /// The caller does not need to set register for the callback, blazor
        /// will handle it as long as the Visible property is bound with 
        /// @bind-Visible="ShowingResults" rather than the 1-way method
        /// of setting Value="ShowingResults"
        /// </summary>
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        [Parameter] required public DocumentListItem? SelectedMeeting { get; set; }

        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly IDocumentService _documentService;

        private Task? _loadTask;

        public MinutesDetails(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IMessenger messenger,
            IDocumentService documentService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _currentUserService = currentUserService;
            _documentService = documentService;
        }

        public MarkupString ErrorMessage { get; set; }
        public bool MinutesAvailable { get; set; } = false;

        string? ReadOnlyUrl { get; set; } = "";

        protected override async Task OnParametersSetAsync()
        {
            if (SelectedMeeting == null)
            {
                return;
            }

            // Start load without awaiting so the spinner can render
            _loadTask = LoadAsync();

            await base.OnParametersSetAsync();
        }

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);

            reset();
        }

        #region Helper Methods

        private void appendWithNewlineIfNeeded(ref string baseString, string appendString)
        {
            if (!string.IsNullOrEmpty(baseString))
            {
                baseString += "\n";
            }
            baseString += appendString;
        }

        private async Task LoadAsync()
        {
            try
            {
                ReadOnlyUrl = await _documentService.GetReadOnlyUrl(SelectedMeeting!.DbKey);
            }
            catch (Exception ex)
            {
                ErrorMessage = new MarkupString(ex.Message);
            }

            MinutesAvailable = true;

            // Ensure UI updates when done
            await InvokeAsync(StateHasChanged);
        }

        private void reset()
        {
            MinutesAvailable = false;
            ReadOnlyUrl = "";
            SelectedMeeting = null;
        }

        #endregion Helper Methods
    }
}
