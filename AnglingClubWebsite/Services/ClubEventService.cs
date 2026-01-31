using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class ClubEventService : DataServiceBase, IClubEventService
    {
        private const string CONTROLLER = "Events";

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

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<ClubEvent>>();
            return content;
        }

    }

}
