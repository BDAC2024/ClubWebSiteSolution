using AnglingClubShared.Entities;
using AnglingClubShared.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class RefDataService : DataServiceBase, IRefDataService
    {
        private static string CONTROLLER = "ReferenceData";

        private readonly ILogger<RefDataService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public RefDataService(
            IHttpClientFactory httpClientFactory,
            ILogger<RefDataService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }

        public async Task<ReferenceData?> ReadReferenceData()
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_REF_DATA}";

            _logger.LogInformation($"ReadReferenceData: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"ReadReferenceData: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<ReferenceData>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ReadReferenceData: {ex.Message}");
                    throw;
                }
            }
        }

    }
}
