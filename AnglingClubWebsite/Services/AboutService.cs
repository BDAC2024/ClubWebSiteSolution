using AnglingClubShared.DTOs;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class AboutService : DataServiceBase, IAboutService
    {
        private static string CONTROLLER = "About";

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

            _logger.LogInformation($"GetAboutInfo: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"GetAboutInfo: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<AboutDto>();
                    content!.API = Http.BaseAddress?.ToString() ?? "Unknown";

                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetAboutInfo: {ex.Message}");
                    throw;
                }
            }
        }

    }

}
