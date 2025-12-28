using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.RichTextEditor;
using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MatchType = AnglingClubShared.Enums.MatchType;

namespace AnglingClubWebsite.Pages
{
    public partial class MatchesViewModel : ViewModelBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ILogger<MatchesViewModel> _logger;
        private readonly IAppDialogService _appDialogService;
        public readonly BrowserService _browserService;
        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;
        private readonly IClubEventService _clubEventService;

        public MatchesViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<MatchesViewModel> logger,
            IAppDialogService appDialogService,
            BrowserService browserService,
            IRefDataService refDataService,
            IGlobalService globalService,
            IClubEventService clubEventService) : base(messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
            _logger = logger;
            _appDialogService = appDialogService;
            messenger.Register<BrowserChange>(this);
            _browserService = browserService;
            _refDataService = refDataService;
            _globalService = globalService;
            _clubEventService = clubEventService;
        }

        [ObservableProperty]
        private bool _isMobile = false;

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private bool _refDataLoaded = false;

        [ObservableProperty]
        private MatchType _selectedMatchType = MatchType.Spring;

        [ObservableProperty]
        private Season _selectedSeason = Season.S20To21;

        [ObservableProperty]
        private ObservableCollection<ClubEvent> _matches = new ObservableCollection<ClubEvent>();

        [ObservableProperty]
        private ObservableCollection<MatchTabData> _matchTabItems = new ObservableCollection<MatchTabData>();

        private List<ClubEvent>? _allMatches = null;

        private List<MatchTabData> _matchTabs = new List<MatchTabData>();

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
            _logger.LogWarning("Loading...");
            await getRefData();
            await base.Loaded();

        }

        private async Task getRefData()
        {
            _messenger.Send(new ShowProgress());

            try
            {
                RefData = await _refDataService.ReadReferenceData();
                SelectedSeason = _globalService.GetStoredSeason(RefData!.CurrentSeason);
                await GetMatches();
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

        public async Task GetMatches()
        {
            _messenger.Send(new ShowProgress());

            try
            {
                _allMatches = await _clubEventService.ReadEventsForSeason(SelectedSeason);
                LoadMatches();
            }
            catch (Exception ex)
            {
                _logger.LogError($"getMatches: {ex.Message}");
            }
            finally
            {
                RefDataLoaded = true;
                _messenger.Send(new HideProgress());
            }
        }

        public void LoadMatches()
        {
      
            if (_allMatches != null)
            {
                Matches = new ObservableCollection<ClubEvent>(_allMatches.Where(m => m.MatchType == SelectedMatchType));
                SetupTabs();
                //this.globalService.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

                //this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
            }
        }

        private void SetupTabs()
        {
            _matchTabs = new List<MatchTabData>
            {
                new MatchTabData { MatchType = MatchType.Spring, Header = "Spring" },
                new MatchTabData { MatchType = MatchType.Club, Header = "Club" },
                new MatchTabData { MatchType = MatchType.Junior, Header = "Junior" },
                new MatchTabData { MatchType = MatchType.OSU, Header = "OSU" },
                new MatchTabData { MatchType = MatchType.Specials, Header = "Specials" },
                new MatchTabData { MatchType = MatchType.Pairs, Header = "Pairs" },
                new MatchTabData { MatchType = MatchType.Evening, Header = "Evening" },
            };

            MatchTabItems = new ObservableCollection<MatchTabData>(_matchTabs);
        }

        public class MatchTabData
        {
            public string Header { get; set; } = "";
            public MatchType MatchType { get; set; } = MatchType.Spring;
        }

    }
}
