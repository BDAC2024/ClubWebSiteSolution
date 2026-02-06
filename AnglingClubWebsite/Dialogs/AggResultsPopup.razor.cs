using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.Dialogs
{
    public partial class AggResultsPopup
    {
        [Parameter] required public bool Visible { get; set; } = false;
        /// <summary>
        /// This is a name-based convention that will trigger a 2-way binding.
        /// The caller does not need to set register for the callback, blazor
        /// will handle it as long as the Visible property is bound with 
        /// @bind-Visible="ShowingResults" rather than the 1-way method
        /// of setting Value="ShowingResults"
        /// </summary>
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        [Parameter] required public int MembershipNumber { get; set; }
        [Parameter] required public AggregateType AggregateType { get; set; }
        [Parameter] required public Season Season { get; set; }
        [Parameter] required public bool BasedOnPoints { get; set; }

        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        private readonly IGlobalService _globalService;
        private readonly BrowserService _browserService;
        private readonly IMatchResultsService _matchResultsService;
        private readonly ILogger<AggResultsPopup> _logger;

        public AggResultsPopup(
            ICurrentUserService currentUserService,
            IGlobalService globalService,
            BrowserService browserService,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            IMatchResultsService matchResultsService,
            ILogger<AggResultsPopup> logger) : base(messenger, currentUserService, authenticationService)
        {
            _globalService = globalService;
            _browserService = browserService;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _matchResultsService = matchResultsService;
            _logger = logger;
        }

        private MemberResultsInSeason _aggResult = new MemberResultsInSeason();
        private bool _resultsLoaded = false;

        private int? _cachedMembershipNumber = 0;

        protected override async Task OnParametersSetAsync()
        {
            if (MembershipNumber == 0)
            {
                return;
            }

            if (_cachedMembershipNumber != MembershipNumber)
            {
                _cachedMembershipNumber = MembershipNumber;
                await GetResults(MembershipNumber, AggregateType, Season, BasedOnPoints);
            }

            await base.OnParametersSetAsync();
        }

        private async Task GetResults(int membershipNumber, AggregateType aggType, Season season, bool basedOnPoints)
        {
            _resultsLoaded = false;
            _aggResult = new MemberResultsInSeason();

            try
            {
                var resultsFromService = await _matchResultsService.GetMemberResultsInSeason(membershipNumber, aggType, season, basedOnPoints);
                _aggResult = resultsFromService ?? new MemberResultsInSeason();
                if (basedOnPoints)
                {
                    _aggResult.ResultsCounted = _aggResult.ResultsCounted.OrderByDescending(x => x.Points).ThenBy(x => x.Date).ToList();
                    _aggResult.ResultsDropped = _aggResult.ResultsDropped.OrderByDescending(x => x.Points).ThenBy(x => x.Date).ToList();
                }
                else
                {
                    _aggResult.ResultsCounted = _aggResult.ResultsCounted.OrderByDescending(x => x.WeightDecimal).ThenBy(x => x.Date).ToList();
                    _aggResult.ResultsDropped = _aggResult.ResultsDropped.OrderByDescending(x => x.WeightDecimal).ThenBy(x => x.Date).ToList();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetResults: {ex.Message}");
            }
            finally
            {
                _resultsLoaded = true;
            }
        }

        //private async Task GetMatchResults(string matchId)
        //{
        //    _resultsLoaded = false;

        //    try
        //    {
        //        var resultsFromService = await _matchResultsService.GetResultsForMatch(matchId);
        //        var results = (resultsFromService ?? new List<MatchResultOutputDto>()).ToList();
        //        _aggResult = results;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"GetMatchResults: {ex.Message}");
        //    }
        //    finally
        //    {
        //        _resultsLoaded = true;
        //    }
        //}

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }
    }
}
