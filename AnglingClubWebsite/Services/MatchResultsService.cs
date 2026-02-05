using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class MatchResultsService : DataServiceBase, IMatchResultsService
    {
        private const string CONTROLLER = "MatchResults";

        private readonly ILogger<MatchResultsService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public MatchResultsService(
            IHttpClientFactory httpClientFactory,
            ILogger<MatchResultsService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }

        public async Task<List<MatchResultOutputDto>?> GetResultsForMatch(string matchId)
        {
            var relativeEndpoint = $"{CONTROLLER}/{matchId}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<MatchResultOutputDto>>();
            return content;
        }

        public async Task<List<LeaguePosition>?> GetLeaguePositions(AggregateType aggType, Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}/standings/{(int)aggType}/{(int)season}";
            var response = await Http.GetAsync($"{relativeEndpoint}");
            var content = await response.Content.ReadFromJsonAsync<List<LeaguePosition>>();
            return content;
        }

        public async Task<List<AggregateWeight>?> GetAggreateWeights(AggregateType aggType, Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}/aggregateWeights/{(int)aggType}/{(int)season}";
            var response = await Http.GetAsync($"{relativeEndpoint}");
            var content = await response.Content.ReadFromJsonAsync<List<AggregateWeight>>();
            return content;
        }

        public async Task<List<TrophyWinner>?> GetTrophyWinners(TrophyType trophyType, Season season)
        {
            var relativeEndpoint = $"trophywinners/{(int)trophyType}/{(int)season}";
            var response = await Http.GetAsync($"{relativeEndpoint}");
            var content = await response.Content.ReadFromJsonAsync<List<TrophyWinner>>();
            return content;
        }


    }

}
