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
using System.ComponentModel;
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

        private List<ClubEvent>? _allMatches = null;
        private List<MatchTabData> _matchTabs = new List<MatchTabData>();

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

            BrowserSize = _browserService.DeviceSize;
        }

        #region Properties

        [ObservableProperty]
        private bool _showCup = false;

        [ObservableProperty]
        private bool _showTime = false;

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private bool _loading = true;

        [ObservableProperty]
        private bool _loadingResults = true;

        [ObservableProperty]
        private MatchType _selectedMatchType = MatchType.Spring;

        [ObservableProperty]
        private int _selectedTab = 0;

        [ObservableProperty]
        private ObservableCollection<ClubEvent> _matches = new ObservableCollection<ClubEvent>();

        [ObservableProperty]
        private bool _showingResults = false;

        [ObservableProperty]
        private ClubEvent? _selectedMatch = null;

        [ObservableProperty]
        private ObservableCollection<MatchTabData> _matchTabItems = new ObservableCollection<MatchTabData>();

        [ObservableProperty]
        private DeviceSize _browserSize = DeviceSize.Unknown;

        [ObservableProperty]
        private bool _browserPortrait = false;

        #endregion Properties

        #region Message Handlers

        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
        }

        #endregion Message Handlers

        #region Methods

        public override async Task Loaded()
        {
            //_logger.LogWarning("Loading...");
            await getInitialData();
            await base.Loaded();

        }

        public bool IsCupVisible()
        {
            return BrowserSize != DeviceSize.Small && ShowCup;
        }

        public bool IsTimeVisible()
        {
            return ShowTime;
        }

        public void LoadMatchesForSelectedType()
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

        public async void ShowResults(ClubEvent match)
        {
            BrowserPortrait = _browserService.IsPortrait;
            _logger.LogWarning($"Selected match {match.Id} on {match.Date.ToShortDateString()}");
            ShowingResults = true;
            SelectedMatch = match;
            _logger.LogWarning($"Portrait {BrowserPortrait}");
        }

        #endregion Methods

        #region Helper Methods

        private void setBrowserDetails()
        {
            BrowserSize = _browserService.DeviceSize;
            BrowserPortrait = _browserService.IsPortrait;
        }

        private async Task getInitialData()
        {
            Loading = true;

            _messenger.Send(new ShowProgress());

            try
            {
                RefData = await _refDataService.ReadReferenceData();
                await GetMatches(_globalService.GetStoredSeason(RefData!.CurrentSeason));
            }
            catch (Exception ex)
            {
                _logger.LogError($"getRefData: {ex.Message}");
            }
            finally
            {
                Loading = false;
                _messenger.Send(new HideProgress());
            }
        }

        private async Task GetMatches(Season season)
        {
            Loading = true;

            _messenger.Send(new ShowProgress());

            //await Task.Delay(2000);

            try
            {
                _allMatches = await _clubEventService.ReadEventsForSeason(season);
                LoadMatchesForSelectedType();
            }
            catch (Exception ex)
            {
                _logger.LogError($"getMatches: {ex.Message}");
            }
            finally
            {
                Loading = false;
                _messenger.Send(new HideProgress());
            }
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

        #endregion Helper Methods

        #region Inter-component Methods

        /// <summary>
        /// This is invoked from the shared SeasonSelector
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        public async Task SeasonChanged(Season season)
        {
            SelectedTab = 0;
            SelectedMatchType = 0;
            await GetMatches(season);
        }

        #endregion Inter-component Methods

        #region Helper Classes

        public class MatchTabData
        {
            public string HeaderFull { get; set; } = "";
            public string HeaderBrief { get; set; } = "";
            public MatchType MatchType { get; set; } = MatchType.Spring;
            public bool Visible { get; set; } = false;
        }

        #endregion Helper Classes

    }
}
