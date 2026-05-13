using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Dialogs
{
    public partial class MatchResultsEntryPopup
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

        [Parameter] required public ClubEvent SelectedMatch { get; set; }

        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        private readonly IGlobalService _globalService;
        private readonly BrowserService _browserService;
        private readonly IMatchResultsService _matchResultsService;
        private readonly ILogger<MatchResultsPopup> _logger;

        public MatchResultsEntryPopup(
            ICurrentUserService currentUserService,
            IGlobalService globalService,
            BrowserService browserService,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            IMatchResultsService matchResultsService,
            ILogger<MatchResultsPopup> logger) : base(messenger, currentUserService, authenticationService)
        {
            _globalService = globalService;
            _browserService = browserService;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _matchResultsService = matchResultsService;
            _logger = logger;
        }

        private List<MatchResultEditDto> _matchResults = new List<MatchResultEditDto>();
        private bool _resultsLoaded = false;
        private bool _showPeg = true;

        private string _cachedMatchId = "";

        protected override async Task OnParametersSetAsync()
        {
            if (SelectedMatch == null)
            {
                return;
            }

            if (_cachedMatchId != SelectedMatch.Id)
            {
                _cachedMatchId = SelectedMatch.Id;
                await GetMatchResults(SelectedMatch.Id);
            }

            await base.OnParametersSetAsync();
        }

        private async Task GetMatchResults(string matchId)
        {
            _resultsLoaded = false;

            try
            {
                var resultsFromService = await _matchResultsService.GetEditableResultsForMatch(matchId);
                var results = (resultsFromService ?? new List<MatchResultEditDto>()).ToList();
                _matchResults = results;
                AddNewRow();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMatchResults: {ex.Message}");
            }
            finally
            {
                _resultsLoaded = true;
            }
        }


        private void NewRowRequired(ChangedEventArgs args)
        {
            if (!_matchResults.First().Pegs.Any(x => x.Peg == ""))
            {
                AddNewRow();
            }
        }

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }

        private void AddNewRow()
        {
            _matchResults.First().Pegs.Add(new MatchResultPegDto
            {
            });

        }
    }
}
