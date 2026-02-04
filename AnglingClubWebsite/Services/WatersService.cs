using AnglingClubShared.DTOs;
using AnglingClubWebsite.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

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

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<WaterOutputDto>>();
            return content;
        }

        public async Task SaveWater(WaterOutputDto water)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_WATERS_UPDATE}";

            WaterUpdateDto dto = new WaterUpdateDto
            {
                DbKey = water.DbKey,
                Description = water.Description,
                Directions = water.Directions
            };
            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", dto);

            return;
        }
    }
}
