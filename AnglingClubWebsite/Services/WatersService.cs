using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Exceptions;
using AnglingClubShared.Models;
using AnglingClubShared.Models.Auth;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;

namespace AnglingClubWebsite.Services
{
    public class WatersService : DataServiceBase, IWatersService
    {
        private static string CONTROLLER = "Waters";

        private readonly ILogger<WatersService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public WatersService(
            IHttpClientFactory httpClientFactory,
            ILogger<WatersService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }


        public async Task<List<WaterOutputDto>?> ReadWaters()
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_WATERS}";

            _logger.LogInformation($"ReadWaters: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"ReadWaters: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else 
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<List<WaterOutputDto>>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ReadWaters: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task SaveWater(WaterOutputDto water)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_WATERS_UPDATE}";

            _logger.LogInformation($"SaveWater: Accessing {Http.BaseAddress}{relativeEndpoint}");

            try
            {
                WaterUpdateDto dto = new WaterUpdateDto 
                { 
                    DbKey = water.DbKey, 
                    Description = water.Description, 
                    Directions = water.Directions 
                };
                var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"SaveWater: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (UserSessionExpiredException)
            {
                _messenger.Send<ShowMessage>(new ShowMessage(AnglingClubShared.Enums.MessageState.Warn, "Session expired", "You must log in again", "OK"));
                await _authenticationService.LogoutAsync();
            }

            return;
        }
    }
}
