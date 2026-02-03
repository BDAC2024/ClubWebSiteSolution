using AnglingClubShared.DTOs;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class AboutService : DataServiceBase, IAboutService
    {
        private const string CONTROLLER = "About";

        private readonly ILogger<AboutService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public AboutService(
            IHttpClientFactory httpClientFactory,
            ILogger<AboutService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }

        public async Task<AboutDto?> GetAboutInfo()
        {
            var relativeEndpoint = $"{CONTROLLER}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<AboutDto>();
            content!.API = Http.BaseAddress?.ToString() ?? "Unknown";

            return content;
        }
    }

}

