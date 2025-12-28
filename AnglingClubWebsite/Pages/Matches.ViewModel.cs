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
        private bool _showCup = false;

        [ObservableProperty]
        private bool _showTime = false;

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private bool _refDataLoaded = false;

        [ObservableProperty]
        private MatchType _selectedMatchType = MatchType.Spring;

        [ObservableProperty]
        private Season _selectedSeason = Season.S20To21;

        [ObservableProperty]
        private int _selectedTab = 0;

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

        public async Task SeasonChanged()
        {
            SelectedTab = 0;
            SelectedMatchType = 0;
            await GetMatches();
        }

        public async Task GetMatches()
        {
            _messenger.Send(new ShowProgress());

            try
            {
                _allMatches = await _clubEventService.ReadEventsForSeason(SelectedSeason);
                _globalService.SetStoredSeason(SelectedSeason);
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
                List<ClubEvent>? selectedMatches = null;

                selectedMatches = _allMatches.Where(m => m.MatchType == SelectedMatchType).ToList();
                Matches = new ObservableCollection<ClubEvent>(selectedMatches);
                ShowCup = selectedMatches.Any(x => x.Cup != "");
                ShowTime = selectedMatches.Any(x => x.Time != "");

                SetupTabs(SelectedMatchType, _allMatches);
                //this.globalService.log("Matches loaded, portrait: " + this.screenService.IsHandsetPortrait);

                //this.setDisplayedColumns(this.screenService.IsHandsetPortrait);
            }
        }

        public bool IsCupVisible(bool isSmall)
        {
            return !isSmall && ShowCup;
        }

        public bool IsTimeVisible(bool isSmall)
        {
            return ShowTime;
        }

        private void SetupTabs(MatchType selectedMatchType, List<ClubEvent> allMatches)
        {
            _matchTabs = new List<MatchTabData>();

            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Spring, HeaderFull = "Spring League", HeaderBrief = "Spring", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Club, HeaderFull = "Club Match", HeaderBrief = "Club", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Junior, HeaderFull = "Junior Match", HeaderBrief = "Junior", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.OSU, HeaderFull = "Ouse/Swale/Ure", HeaderBrief = "OSU", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Specials, HeaderFull = "Specials", HeaderBrief = "Specials", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Pairs, HeaderFull = "Pairs", HeaderBrief = "Pairs", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Evening, HeaderFull = "Evening", HeaderBrief = "Evening", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Visitors, HeaderFull = "Visiting Clubs", HeaderBrief = "Visitors", });
            addMatchTab(allMatches, _matchTabs, new MatchTabData { MatchType = MatchType.Qualifier, HeaderFull = "Event Qualifiers", HeaderBrief = "Qualifiers", });

            MatchTabItems = new ObservableCollection<MatchTabData>(_matchTabs);

            var i = 0;
            SelectedTab = 0;
            foreach (var item in _matchTabs)
            {
                if (item.MatchType == selectedMatchType)
                {
                    SelectedTab = i;
                }
                i++;
            }
        }

        private void addMatchTab(List<ClubEvent> allMatches, List<MatchTabData> matchTabs, MatchTabData tabData)
        {
            if (allMatches.Any(x => x.MatchType == tabData.MatchType))
            {
                matchTabs.Add(tabData);
            }
        }

        public class MatchTabData
        {
            public string HeaderFull { get; set; } = "";
            public string HeaderBrief { get; set; } = "";
            public MatchType MatchType { get; set; } = MatchType.Spring;
            public bool Visible { get; set; } = false;
        }

    }
}
