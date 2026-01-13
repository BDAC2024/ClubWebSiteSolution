using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Grids;

namespace AnglingClubWebsite.Pages
{
    public partial class MeetingMinutes : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly IDocumentService _documentService;
        private readonly BrowserService _browserService;
        private readonly IDialogQueue _dialogQueue;

        public MeetingMinutes(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService,
                        IMessenger messenger,
                        IDocumentService documentService,
                        BrowserService browserService,
                        IDialogQueue dialogQueue) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _documentService = documentService;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);

            BrowserSize = _browserService.DeviceSize;
            _dialogQueue = dialogQueue;
        }

        #region Properties
        private SfGrid<DocumentListItem>? Grid;

        public bool AddingMinutes = false;
        public bool ShowingMeeting = false;

        public List<DocumentListItem> Documents { get; set; } = new List<DocumentListItem>();

        public DocumentListItem? SelectedMeeting { get; set; }

        public DeviceSize BrowserSize = DeviceSize.Unknown;

        public bool DataLoaded { get; set; } = false;

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            await ReadMeetings();
            DataLoaded = true;

            await base.OnParametersSetAsync();
        }

        public async Task DataboundHandler(object args)
        {
            await this.Grid!.AutoFitColumnsAsync();
        }

        public bool IsWide()
        {
            return BrowserSize != DeviceSize.Small;
        }

        public async Task AddMinutesHandler()
        {
            AddingMinutes = true;
        }


        public void MeetingSelectedHandler(RowSelectEventArgs<DocumentListItem> args)
        {
            SelectedMeeting = args.Data;
            ShowingMeeting = true;
        }

        private async Task DownloadAsync(DocumentListItem doc)
        {
            //await DocumentService.DownloadAsync(doc.Id);
        }

        private async Task DeleteAsync(DocumentListItem doc)
        {
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Confirm,
                Severity = DialogSeverity.Warn,
                Title = "Please confirm",
                Message = $"Do you really want to delete the minutes for '{doc.Title}' on {doc.Created.ToString("dd MMM yy")}?",
                CancelText = "Cancel",
                ConfirmText = "Yes",
                OnConfirmAsync = async () =>
                {
                    try
                    {
                        await _documentService.DeleteDocument(doc.DbKey);
                        await RefreshGridAsync();

                        _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Success, "Success", "Requested minutes have been deleted"));
                    }
                    catch (Exception)
                    {
                        _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Error, "Deletion Failed", "Unable to delete requested minutes"));
                    }
                }
            });
        }

        private async Task RefreshGridAsync()
        {
            // Option A: re-query and rebind
            await ReadMeetings();
            StateHasChanged();

            // Option B (often useful as well): force Syncfusion to re-render its view
            if (Grid is not null)
            {
                await Grid.Refresh();
            }
        }

        public void Receive(BrowserChange message)
        {
            BrowserSize = _browserService.DeviceSize;
        }

        #endregion Events

        private async Task ReadMeetings()
        {
            Documents = await _documentService.ReadDocuments(DocumentType.MeetingMinutes) ?? new List<DocumentListItem>();
        }
    }
}
