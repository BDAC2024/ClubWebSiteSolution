using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.RichTextEditor;

namespace AnglingClubWebsite.Pages
{
    public partial class MeetingMinutes : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly IDocumentService _documentService;
        private readonly BrowserService _browserService;

        public MeetingMinutes(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService,
                        IMessenger messenger,
                        IDocumentService documentService,
                        BrowserService browserService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _documentService = documentService;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);

            BrowserSize = _browserService.DeviceSize;
        }

        #region Properties
        private SfGrid<DocumentListItem>? Grid;

        public bool AddingMinutes = false;

        public List<DocumentListItem> Documents { get; set; } = new List<DocumentListItem>();

        public DeviceSize BrowserSize = DeviceSize.Unknown;

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            await ReadMeetings();

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
