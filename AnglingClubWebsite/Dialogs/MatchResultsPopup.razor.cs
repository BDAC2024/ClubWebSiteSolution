using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Extensions;
using AnglingClubWebsite.Pages;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace AnglingClubWebsite.Dialogs
{
    public partial class MatchResultsPopup
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

        public MatchResultsPopup(
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

        private List<MatchResultOutputDto> _matchResults = new List<MatchResultOutputDto>();
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
                var resultsFromService = await _matchResultsService.GetResultsForMatch(matchId);
                var results = (resultsFromService ?? new List<MatchResultOutputDto>()).ToList();
                _matchResults = results;
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

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }
    }
}
