using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Dialogs
{
    public partial class AddMinutes
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


        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        private UploadFiles? _meetingMinutesFile;

        public AddMinutes(
            ICurrentUserService currentUserService, 
            IAuthenticationService authenticationService, 
            IMessenger messenger) : base (messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

        public DocumentMeta DocumentInfo { get; set; } = new DocumentMeta() { Created = DateTime.Now };
        public string ErrorMessage { get; set; } = "";


        protected override async Task OnParametersSetAsync()
        {
            reset();

            await base.OnParametersSetAsync();
        }

        private async Task UploadHandler(UploadChangeEventArgs args)
        {
            if (args.Files.Any())
            {
                ErrorMessage = "";
                _meetingMinutesFile = args.Files.First();
                DocumentInfo.Name = _meetingMinutesFile.FileInfo.Name;
            }
        }

        private async Task RemoveHandler()
        {
            reset();
        }

        private async Task SaveAsync()
        {
            ErrorMessage = "";

            if (_meetingMinutesFile == null)
            {
                ErrorMessage = "You must provide a file";
                return;
            }

            DocumentInfo.DocumentType = DocumentType.MeetingMinutes;

            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }

        private void reset()
        {
            _meetingMinutesFile = null;
            ErrorMessage = "";
        }
    }
}
