using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
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

        // Signal to parent: “refresh your grid”
        [Parameter] public EventCallback RefreshRequested { get; set; }

        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly IDocumentService _documentService;

        private UploadFiles? _meetingMinutesFile;

        public AddMinutes(
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

        public DocumentMeta DocumentInfo { get; set; } = new DocumentMeta() { Created = DateTime.Now };
        public MarkupString ErrorMessage { get; set; }
        public bool Uploading { get; set; } = false;


        protected override async Task OnParametersSetAsync()
        {
            reset();

            DocumentInfo.Created = DateTime.Now;
            DocumentInfo.Title = "";
            DocumentInfo.Notes = "";
            DocumentInfo.OriginalFileName = "";
            DocumentInfo.StoredFileName = "";

            await base.OnParametersSetAsync();
        }

        private async Task UploadHandler(UploadChangeEventArgs args)
        {
            if (args.Files.Any())
            {
                ErrorMessage = new MarkupString(string.Empty);
                _meetingMinutesFile = args.Files.First();
                DocumentInfo.OriginalFileName = _meetingMinutesFile.FileInfo.Name;
            }
        }

        private async Task RemoveHandler()
        {
            reset();
        }

        private async Task SaveAsync()
        {
            ErrorMessage = new MarkupString(string.Empty);
            var errString = "";

            if (_meetingMinutesFile == null)
            {
                appendWithNewlineIfNeeded(ref errString, "You must provide a file");
            }
            if (DocumentInfo.Title.IsWhiteSpace())
            {
                appendWithNewlineIfNeeded(ref errString, "You must provide a Title");
            }

            ErrorMessage = new MarkupString(
            errString
                .Replace("\n", "<br />"));

            if (!string.IsNullOrEmpty(ErrorMessage.Value))
            {
                return;
            }

            Uploading = true;

            DocumentInfo.DocumentType = DocumentType.MeetingMinutes;

            try
            {
                var uploadUrlDetails = await _documentService.GetDocumentUploadUrl(_meetingMinutesFile!, DocumentInfo.DocumentType);

                if (uploadUrlDetails == null)
                {
                    ErrorMessage = new MarkupString("There was an error getting the upload URL. Please try again later.");
                    return;
                }

                DocumentInfo.StoredFileName = uploadUrlDetails.UploadedFileName;
                DocumentInfo.Searchable = true;

                // Store the uploaded doc 
                await _documentService.UploadDocumentWithPresignedUrl(uploadUrlDetails.UploadUrl, _meetingMinutesFile!);

                // Create a doc record in the database
                await _documentService.SaveDocument(DocumentInfo);

                // Tell the parent to update its source of truth
                await VisibleChanged.InvokeAsync(false);

                // Tell parent to refresh
                await RefreshRequested.InvokeAsync();
            }
            catch (Exception)
            {
                _messenger.Send(new ShowMessage(MessageState.Warn, "Save failed", "Unable to save the docunent"));
                Uploading = false;
            }

        }

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
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

        private void reset()
        {
            Uploading = false;
            _meetingMinutesFile = null;
            ErrorMessage = new MarkupString(string.Empty);
        }

        #endregion Helper Methods
    }
}
