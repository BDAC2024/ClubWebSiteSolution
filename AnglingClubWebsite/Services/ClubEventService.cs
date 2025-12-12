using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class ClubEventService : DataServiceBase, IClubEventService
    {
        private static string CONTROLLER = "Events";

        private readonly ILogger<ClubEventService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public ClubEventService(
            IHttpClientFactory httpClientFactory,
            ILogger<ClubEventService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }

        public async Task<List<ClubEvent>?> ReadEventsForSeason(Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_CLUB_EVENTS}/{season}";

            _logger.LogInformation($"ReadEventsForSeason: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"ReadEventsForSeason: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<List<ClubEvent>>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ReadEventsForSeason: {ex.Message}");
                    throw;
                }
            }
        }

    }

}
