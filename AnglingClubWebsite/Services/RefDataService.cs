using AnglingClubShared.Entities;
using AnglingClubShared.Models;
using AnglingClubShared.Models.Auth;
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

        private ReferenceData? _cachedData = null;

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

        public async Task InitializeAsync()
        {
            if (_cachedData == null)
            {
                _cachedData = await LoadReferenceData();
            }
        }

        public async Task<ReferenceData?> ReadReferenceData()
        {
            if (_cachedData == null)
            {
                _cachedData = await LoadReferenceData();
            }

            return _cachedData;
        }

        public async Task<ReferenceData?> LoadReferenceData()
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_REF_DATA}";

            _logger.LogInformation($"LoadReferenceData: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"LoadReferenceData: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
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
                    _logger.LogError($"LoadReferenceData: {ex.Message}");
                    throw;
                }
            }
        }

    }
}
