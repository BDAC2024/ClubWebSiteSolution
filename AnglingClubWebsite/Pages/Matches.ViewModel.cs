using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.RichTextEditor;
using System.Collections.ObjectModel;

namespace AnglingClubWebsite.Pages
{
    public partial class MatchesViewModel : ViewModelBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ILogger<MatchesViewModel> _logger;
        private readonly IAppDialogService _appDialogService;
        private readonly BrowserService _browserService;
        private readonly IRefDataService _refDataService;

        public MatchesViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<MatchesViewModel> logger,
            IAppDialogService appDialogService,
            BrowserService browserService,
            IRefDataService refDataService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _logger = logger;
            _appDialogService = appDialogService;
            messenger.Register<BrowserChange>(this);
            _browserService = browserService;
            _refDataService = refDataService;
        }

        [ObservableProperty]
        private bool _isMobile = false;

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private bool _refDataLoaded = false;

        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
        }

        private void setBrowserDetails()
        {
            IsMobile = _browserService.IsMobile;

        }

        public override async Task Loaded()
        {
            await getRefData();
            await base.Loaded();
        }

        private async Task getRefData()
        {
            _messenger.Send(new ShowProgress());

            try
            {
                RefData = await _refDataService.ReadReferenceData();

            }
            catch (Exception ex)
            {
                _logger.LogError($"getRefData: {ex.Message}");
            }
            finally
            {
                RefDataLoaded = true;
                _messenger.Send(new HideProgress());
            }
        }

    }
}
