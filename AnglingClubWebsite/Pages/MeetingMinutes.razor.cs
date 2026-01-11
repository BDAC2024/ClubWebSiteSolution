using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class MeetingMinutes
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly IDocumentService _documentService;

        public MeetingMinutes(
                        ICurrentUserService currentUserService,
                        IAuthenticationService authenticationService,
                        IMessenger messenger,
                        IDocumentService documentService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _documentService = documentService;
        }

        #region Properties

        public bool AddingMinutes = false;

        public List<DocumentListItem> Documents { get; set; } = new List<DocumentListItem>();

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            Documents = await _documentService.ReadDocuments(DocumentType.MeetingMinutes) ?? new List<DocumentListItem>();

            await base.OnParametersSetAsync();
        }

        public async Task AddMinutesHandler()
        {
            AddingMinutes = true;
        }

        #endregion Events
    }
}
