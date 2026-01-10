using AnglingClubShared.DTOs;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class MatchResultsService : DataServiceBase, IMatchResultsService
    {
        private static string CONTROLLER = "MatchResults";

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

            _logger.LogInformation($"GetResultsForMatch: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"GetResultsForMatch: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<List<MatchResultOutputDto>>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetResultsForMatch: {ex.Message}");
                    throw;
                }
            }
        }

    }

}
