using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class StandingsLeague : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly BrowserService _browserService;
        public readonly IGlobalService GlobalService;
        private readonly IMatchResultsService _matchResultsService;
        private readonly IClubEventService _clubEventService;
        private readonly ILogger<StandingsLeague> _logger;
        private readonly IRefDataService _refDataService;

        private List<ClubEvent>? _allMatches = null;
        private List<TabData> _matchTabs = new List<TabData>();

        public StandingsLeague(IAuthenticationService authenticationService,
                         IMessenger messenger,
                         ICurrentUserService currentUserService,
                         BrowserService browserService,
                         IGlobalService globalService,
                         IMatchResultsService matchResultsService,
                         IClubEventService clubEventService,
                         ILogger<StandingsLeague> logger,
                         IRefDataService refDataService) : base(messenger, currentUserService, authenticationService)
        {
            messenger.Register<BrowserChange>(this);

            _authenticationService = authenticationService;
            _messenger = messenger;
            _currentUserService = currentUserService;
            _browserService = browserService;
            GlobalService = globalService;
            _matchResultsService = matchResultsService;
            _clubEventService = clubEventService;
            _logger = logger;
            _refDataService = refDataService;
        }

        #region Properties

        public DeviceSize BrowserSize = DeviceSize.Unknown;

        public int SelectedTab { get; set; } = 0;
        public AggregateType SelectedAggType { get; set; } = AggregateType.Spring;

        public bool TabsLoaded { get; set; } = false;
        public bool StandingsLoaded { get; set; } = false;

        public List<TabData> MatchTabItems = new List<TabData>();

        public List<LeaguePosition>? SeasonStandings { get; set; } = new List<LeaguePosition>();
        public IQueryable<LeaguePosition>? SeasonStandingsQueryable;

        public bool ShowWeight { get; set; } = true;

        public ReferenceData? RefData;

        #endregion Properties

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            await getInitialData();
            TabsLoaded = true;

            await base.OnParametersSetAsync();
        }

        public void Receive(BrowserChange message)
        {
            BrowserSize = _browserService.DeviceSize;
        }

        public async Task OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;

            var selected = MatchTabItems.ToArray()[args.SelectedIndex].AggregateType;
            //Console.WriteLine($"Selected item: {args.SelectedIndex} - {selected}");
            SelectedAggType = selected;
            ShowWeight = selected != AggregateType.OSU;
            await loadLeague(GlobalService.GetStoredSeason(EnumUtils.CurrentSeason()));
        }

        /// <summary>
        /// This is invoked from the shared SeasonSelector
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        public async Task SeasonChanged(Season season)
        {
            SelectedTab = 0;
            SelectedAggType = 0;
            StandingsLoaded = false;
            await getMatches(season);
        }

        private async Task getMatches(Season season)
        {
            try
            {
                var m = await _clubEventService.ReadEventsForSeason(season);
                if (m != null)
                {
                    _allMatches = m.Where(x => x.EventType == EventType.Match).ToList();
                    setupTabs(_allMatches);
                }
                await loadLeague(season);
            }
            catch (Exception ex)
            {
                _logger.LogError($"getMatches: {ex.Message}");
            }
        }

        #endregion Events

        #region Helper Methods

        private async Task getInitialData()
        {
            TabsLoaded = false;

            try
            {
                RefData = await _refDataService.ReadReferenceData();
                await getMatches(GlobalService.GetStoredSeason(RefData!.CurrentSeason));
            }
            catch (Exception ex)
            {
                _logger.LogError($"getRefData: {ex.Message}");
            }
            finally
            {
                TabsLoaded = true;
            }
        }

        private async Task loadLeague(Season season)
        {
            StandingsLoaded = false;

            SeasonStandings = await _matchResultsService.GetLeaguePositions(SelectedAggType, season);

            if (SeasonStandings != null)
            {
                SeasonStandingsQueryable = SeasonStandings.AsQueryable();
            }

            StandingsLoaded = true;

        }

        private void setupTabs(List<ClubEvent> allMatches)
        {
            _matchTabs = new List<TabData>();

            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.Spring, HeaderFull = "Spring League", HeaderBrief = "Spring", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.ClubRiver, HeaderFull = "Club Match - River", HeaderBrief = "Club/River", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.ClubPond, HeaderFull = "Club Match - Pond", HeaderBrief = "Club/Pond", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.PairsPointsAsc, HeaderFull = "Pairs", HeaderBrief = "Pairs", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.Junior, HeaderFull = "Junior", HeaderBrief = "Junior", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.OSU, HeaderFull = "Ouse/Swale/Ure", HeaderBrief = "OSU", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.Evening, HeaderFull = "Evening", HeaderBrief = "Evening", });

            MatchTabItems = new List<TabData>(_matchTabs);

            SelectedTab = 0;
        }

        private void addTab(List<ClubEvent> allMatches, List<TabData> matchTabs, TabData tabData)
        {
            if (allMatches.Any(x => x.AggregateType == tabData.AggregateType))
            {
                matchTabs.Add(tabData);
            }
        }

        #endregion Helper Methods

    }
}
