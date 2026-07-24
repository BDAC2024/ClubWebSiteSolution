using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.QuickGrid;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class Matches : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly ILogger<Matches> _logger;
        private readonly BrowserService _browserService;
        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;
        private readonly IClubEventService _clubEventService;

        private List<ClubEvent> _allMatches = new();

        public Matches(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<Matches> logger,
            BrowserService browserService,
            IRefDataService refDataService,
            IGlobalService globalService,
            IClubEventService clubEventService) : base(messenger, currentUserService, authenticationService)
        {
            _logger = logger;
            _browserService = browserService;
            _refDataService = refDataService;
            _globalService = globalService;
            _clubEventService = clubEventService;

            messenger.Register<BrowserChange>(this);
            SetBrowserDetails();
        }

        public bool ShowCup { get; set; }
        public bool DataLoaded { get; set; }
        public MatchType SelectedMatchType { get; set; } = MatchType.Spring;
        public int SelectedTab { get; set; }
        public List<ClubEvent> Matches { get; set; } = new();
        public IQueryable<ClubEvent>? MatchesQueryable { get; set; }
        public bool ShowingResults { get; set; }
        public ClubEvent SelectedMatch { get; set; } = new();
        public List<TabData> MatchTabItems { get; set; } = new();
        public DeviceSize BrowserSize { get; set; } = DeviceSize.Unknown;

        public static readonly GridSort<ClubEvent> SortByDate =
            GridSort<ClubEvent>.ByAscending(x => x.Date);

        public override async Task Loaded()
        {
            await LoadInitialData();
            await base.Loaded();
        }

        public void Receive(BrowserChange message)
        {
            SetBrowserDetails();
        }

        public void OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;
            SelectedMatchType = MatchTabItems[args.SelectedIndex].MatchType;
            LoadMatchesForSelectedType();
        }

        public async Task SeasonChanged(Season? season)
        {
            if (season is null)
            {
                return;
            }

            SelectedTab = 0;
            SelectedMatchType = MatchType.Spring;
            _globalService.SetStoredSeason(season.Value);
            await LoadMatches(season.Value);
        }

        public bool IsCupVisible()
        {
            return BrowserSize != DeviceSize.Small && ShowCup;
        }

        public void MatchSelectedHandler(ClubEvent row)
        {
            SelectedMatch = row;
            ShowingResults = true;
        }

        public string CellClass(ClubEvent row)
        {
            var classes = "bdac-rowcell";

            if (ReferenceEquals(SelectedMatch, row))
            {
                classes += " bdac-row-selected";
            }

            if (row.Date.Date < DateTime.Today)
            {
                classes += " bdac-row-past";
            }

            return classes;
        }

        private void SetBrowserDetails()
        {
            BrowserSize = _browserService.DeviceSize;
        }

        private async Task LoadInitialData()
        {
            DataLoaded = false;

            try
            {
                var refData = await _refDataService.ReadReferenceData();
                await LoadMatches(_globalService.GetStoredSeason(refData!.CurrentSeason));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load match reference data");
            }
            finally
            {
                DataLoaded = true;
            }
        }

        private async Task LoadMatches(Season season)
        {
            DataLoaded = false;

            try
            {
                _allMatches = await _clubEventService.ReadEventsForSeason(season);
                LoadMatchesForSelectedType();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load matches for {Season}", season);
            }
            finally
            {
                DataLoaded = true;
            }
        }

        private void LoadMatchesForSelectedType()
        {
            Matches = _allMatches.Where(match => match.MatchType == SelectedMatchType).ToList();
            MatchesQueryable = Matches.AsQueryable();
            ShowCup = Matches.Any(match => !string.IsNullOrEmpty(match.Cup));
            SetupTabs();
        }

        private void SetupTabs()
        {
            var availableTabs = new[]
            {
                new TabData { MatchType = MatchType.Spring, HeaderFull = "Spring League", HeaderBrief = "Spring" },
                new TabData { MatchType = MatchType.Club, HeaderFull = "Club Match", HeaderBrief = "Club" },
                new TabData { MatchType = MatchType.Junior, HeaderFull = "Junior Match", HeaderBrief = "Junior" },
                new TabData { MatchType = MatchType.Specials, HeaderFull = "Specials", HeaderBrief = "Specials" },
                new TabData { MatchType = MatchType.Pairs, HeaderFull = "Pairs", HeaderBrief = "Pairs" },
                new TabData { MatchType = MatchType.Evening, HeaderFull = "Evening", HeaderBrief = "Evening" },
                new TabData { MatchType = MatchType.Midweek, HeaderFull = "Midweek", HeaderBrief = "Midweek" },
                new TabData { MatchType = MatchType.Visitors, HeaderFull = "Visiting Clubs", HeaderBrief = "Visitors" },
                new TabData { MatchType = MatchType.OSU, HeaderFull = "Ouse/Swale/Ure", HeaderBrief = "OSU" },
                new TabData { MatchType = MatchType.Qualifier, HeaderFull = "Event Qualifiers", HeaderBrief = "Qualifiers" }
            };

            MatchTabItems = availableTabs
                .Where(tab => _allMatches.Any(match => match.MatchType == tab.MatchType))
                .ToList();
            SelectedTab = Math.Max(0, MatchTabItems.FindIndex(tab => tab.MatchType == SelectedMatchType));
        }
    }
}
