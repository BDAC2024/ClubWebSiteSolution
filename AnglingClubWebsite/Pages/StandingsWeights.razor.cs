using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubShared.Services;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class StandingsWeights : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly BrowserService _browserService;
        public readonly IGlobalService GlobalService;
        private readonly IMatchResultsService _matchResultsService;
        private readonly IClubEventService _clubEventService;
        private readonly ILogger<StandingsWeights> _logger;
        private readonly IRefDataService _refDataService;

        private List<ClubEvent>? _allMatches = null;
        private List<TabData> _matchTabs = new List<TabData>();

        public StandingsWeights(IAuthenticationService authenticationService,
                         IMessenger messenger,
                         ICurrentUserService currentUserService,
                         BrowserService browserService,
                         IGlobalService globalService,
                         IMatchResultsService matchResultsService,
                         IClubEventService clubEventService,
                         ILogger<StandingsWeights> logger,
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

            setBrowserDetails();
        }

        #region Properties

        public string AboutInfo { get; set; } = "";
        public DeviceSize BrowserSize = DeviceSize.Unknown;

        public int SelectedTab { get; set; } = 0;
        public AggregateType SelectedAggType { get; set; } = AggregateType.Spring;
        public Season SelectedSeason { get; set; }
        public int SelectedMembershipNumber { get; set; }

        public bool TabsLoaded { get; set; } = false;
        public bool StandingsLoaded { get; set; } = false;

        public List<TabData> MatchTabItems = new List<TabData>();

        public List<AggregateWeight>? AggWeights { get; set; } = new List<AggregateWeight>();
        public IQueryable<AggregateWeight>? AggWeightsQueryable;

        public bool ShowingResults { get; set; } = false;

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
            setBrowserDetails();
        }

        public async Task OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;

            var selected = MatchTabItems.ToArray()[args.SelectedIndex].AggregateType;
            //Console.WriteLine($"Selected item: {args.SelectedIndex} - {selected}");
            SelectedAggType = selected;
            SelectedMembershipNumber = 0;
            await loadLeague(GlobalService.GetStoredSeason(EnumUtils.CurrentSeason()));
        }

        /// <summary>
        /// This is invoked from the shared SeasonSelector
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        public async Task SeasonChanged(Season? season)
        {
            SelectedSeason = season!.Value;
            SelectedTab = 0;
            SelectedMembershipNumber = 0;
            SelectedAggType = 0;
            StandingsLoaded = false;
            await getMatches(SelectedSeason);
            StateHasChanged();
        }

        public void AnglerSelectedHandler(AggregateWeight row)
        {
            SelectedMembershipNumber = row.MembershipNumber;
            ShowingResults = true;
        }

        #endregion Events

        #region Helper Methods

        private async Task getInitialData()
        {
            TabsLoaded = false;

            try
            {
                RefData = await _refDataService.ReadReferenceData();
                SelectedSeason = GlobalService.GetStoredSeason(RefData!.CurrentSeason);
                await getMatches(SelectedSeason);
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

        private async Task getMatches(Season season)
        {
            TabsLoaded = false;
            MatchTabItems = new List<TabData>();

            try
            {
                var m = await _clubEventService.ReadEventsForSeason(season);
                if (m != null && m.Any())
                {
                    _allMatches = m.Where(x => x.EventType == EventType.Match).ToList();
                    setupTabs(_allMatches);
                    await loadLeague(season);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"getMatches: {ex.Message}");
            }
            finally
            {
                TabsLoaded = true;
            }

        }

        private async Task loadLeague(Season season)
        {
            StandingsLoaded = false;

            AggWeights = await _matchResultsService.GetAggreateWeights(SelectedAggType, season);

            if (AggWeights != null)
            {
                AggWeightsQueryable = AggWeights.AsQueryable();

                setupAboutInfo(AggWeights);
            }

            StandingsLoaded = true;

        }

        private void setupAboutInfo(List<AggregateWeight> aggWeights)
        {
            AboutInfo = "";
            var dropCount = MatchHelperService.MatchesToBeDropped(SelectedAggType, SelectedSeason);

            if (dropCount > 0)
            {
                AboutInfo = $"Anglers best {aggWeights.First().MatchesInSeason - dropCount} weights count from all {aggWeights.First().MatchesInSeason} matches.";
            }
        }

        private void setupTabs(List<ClubEvent> allMatches)
        {
            _matchTabs = new List<TabData>();

            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.Spring, HeaderFull = "Spring League", HeaderBrief = "Spring", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.ClubRiver, HeaderFull = "Club Match - River", HeaderBrief = "Club/River", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.ClubPond, HeaderFull = "Club Match - Pond", HeaderBrief = "Club/Pond", });
            addTab(allMatches, _matchTabs, new TabData { AggregateType = AggregateType.Pairs, HeaderFull = "Pairs", HeaderBrief = "Pairs", });

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

        private void setBrowserDetails()
        {
            BrowserSize = _browserService.DeviceSize;
        }

        #endregion Helper Methods

    }
}
