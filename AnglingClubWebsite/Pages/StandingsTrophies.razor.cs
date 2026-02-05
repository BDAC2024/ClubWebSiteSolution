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
    public partial class StandingsTrophies : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly BrowserService _browserService;
        public readonly IGlobalService GlobalService;
        private readonly IMatchResultsService _matchResultsService;
        private readonly IClubEventService _clubEventService;
        private readonly ILogger<StandingsTrophies> _logger;
        private readonly IRefDataService _refDataService;

        private List<ClubEvent>? _allMatches = null;
        private List<TabData> _matchTabs = new List<TabData>();

        public StandingsTrophies(IAuthenticationService authenticationService,
                         IMessenger messenger,
                         ICurrentUserService currentUserService,
                         BrowserService browserService,
                         IGlobalService globalService,
                         IMatchResultsService matchResultsService,
                         IClubEventService clubEventService,
                         ILogger<StandingsTrophies> logger,
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
        public TrophyType SelectedType { get; set; } = TrophyType.Senior;

        public bool TabsLoaded { get; set; } = false;
        public bool TrophiesLoaded { get; set; } = false;

        public List<TabData> TabItems = new List<TabData>();

        public List<TrophyWinner>? TrophyWinners { get; set; } = new List<TrophyWinner>();
        public IQueryable<TrophyWinner>? TrophyWinnersQueryable;

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

            var selected = TabItems.ToArray()[args.SelectedIndex].TrophyType;
            //Console.WriteLine($"Selected item: {args.SelectedIndex} - {selected}");
            SelectedType = selected;
            await loadTrophyWinners(GlobalService.GetStoredSeason(EnumUtils.CurrentSeason()));
        }

        /// <summary>
        /// This is invoked from the shared SeasonSelector
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        public async Task SeasonChanged(Season? season)
        {
            SelectedTab = 0;
            SelectedType = 0;
            TrophiesLoaded = false;
            await loadTrophyWinners(season!.Value);
            StateHasChanged();
        }


        #endregion Events

        #region Helper Methods

        public string CellClass(TrophyWinner row)
        {
            var classes = "bdac-rowcell";

            // Dim rows where still running (could still be won in future)
            if (row.IsRunning)
            {
                classes += " bdac-row-still-running";
            }

            return classes;
        }

        private async Task getInitialData()
        {
            TabsLoaded = false;

            try
            {
                RefData = await _refDataService.ReadReferenceData();
                setupTabs();
                await loadTrophyWinners(GlobalService.GetStoredSeason(RefData!.CurrentSeason));
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

        private async Task loadTrophyWinners(Season season)
        {
            TrophiesLoaded = false;

            TrophyWinners = await _matchResultsService.GetTrophyWinners(SelectedType, season);

            if (TrophyWinners != null)
            {
                TrophyWinnersQueryable = TrophyWinners.AsQueryable();
            }

            TrophiesLoaded = true;

        }

        private void setupTabs()
        {
            TabItems = new List<TabData>()
            {
                new TabData { TrophyType = TrophyType.Senior, HeaderFull = "Senior", HeaderBrief = "Senior", },
                new TabData { TrophyType = TrophyType.Junior, HeaderFull = "Junior", HeaderBrief = "Junior", }
            };

            SelectedTab = 0;
        }

        #endregion Helper Methods

    }
}
