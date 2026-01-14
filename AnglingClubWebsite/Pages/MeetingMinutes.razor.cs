using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using System.Linq.Expressions;

namespace AnglingClubWebsite.Pages
{
    public partial class MeetingMinutes : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly IDocumentService _documentService;
        private readonly BrowserService _browserService;
        private readonly IDialogQueue _dialogQueue;
        private readonly INavigationService _navigationService;

        public MeetingMinutes(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService,
                        IMessenger messenger,
                        IDocumentService documentService,
                        BrowserService browserService,
                        IDialogQueue dialogQueue,
                        INavigationService navigationService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _documentService = documentService;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);

            BrowserSize = _browserService.DeviceSize;
            _dialogQueue = dialogQueue;
            _navigationService = navigationService;
        }

        #region Properties
        private SfGrid<DocumentListItem>? Grid;
        private SfTextBox? SearchComponent { get; set; }

        public bool AddingMinutes = false;
        public bool ShowingMeeting = false;

        public List<DocumentListItem> Documents { get; set; } = new List<DocumentListItem>();

        public string SearchText { get; set; } = "";
        private SearchModel _model = new();
        private EditContext? _editContext;
        private ValidationMessageStore? _messages;
        public DocumentListItem? SelectedMeeting { get; set; }

        public DeviceSize BrowserSize = DeviceSize.Unknown;

        public bool DataLoaded { get; set; } = false;

        #endregion Properties

        #region Events

        protected override void OnInitialized()
        {
            _editContext = new EditContext(_model);
            _messages = new ValidationMessageStore(_editContext);
        }

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

        private async void AddSearchIcon()
        {
            if (SearchComponent != null)
            {
                //Add icon to the TextBox
                await SearchComponent.AddIconAsync("append", "e-icons e-search");
            }
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
            try
            {
                var url = await _documentService.Download(doc.DbKey);

                if (url != null)
                {
                    _navigationService.NavigateTo(url, forceLoad: true);
                }
                else
                {
                    _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Error, "Download Failed", "Unable to download requested minutes"));
                }
            }
            catch (Exception)
            {
                _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Error, "Download Failed", "Unable to download requested minutes"));
            }
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

        private async Task Search(ChangedEventArgs args)
        {
            _messages!.Clear();

            if (!string.IsNullOrWhiteSpace(_model.SearchText) &&
                _model.SearchText.Length < 3)
            {
                _messages.Add(
                    () => _model.SearchText,
                    "Must be at least 3 characters"
                );
            }

            _editContext!.NotifyValidationStateChanged();
            //_messenger.Send<ShowMessage>(new ShowMessage(MessageState.Info, "You entered", args.Value));
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

        #region Helper Classes

        public class SearchModel
        {
            public string? SearchText { get; set; }
        }

        #endregion Helper Classes
    }
}
