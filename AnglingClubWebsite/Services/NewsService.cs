using AnglingClubShared.Entities;
using AnglingClubShared.Exceptions;
using AnglingClubWebsite.Models;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class NewsService : DataServiceBase, INewsService
    {
        private static string CONTROLLER = "News";

        private readonly ILogger<NewsService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public NewsService(
            IHttpClientFactory httpClientFactory,
            ILogger<NewsService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }


        public async Task<List<NewsItem>?> ReadNews()
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_NEWS}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<NewsItem>>();
            return content;
        }


        public async Task DeleteNewsItem(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_NEWS}/{id}";

            var response = await Http.DeleteAsync($"{relativeEndpoint}");

        }

        public async Task SaveNewsItem(NewsItem item)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_NEWS}";

            _logger.LogInformation($"SaveNewsItem: Accessing {Http.BaseAddress}{relativeEndpoint}");

            try
            {
                var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", new List<NewsItem> { item });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"SaveNewsItem: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (UserSessionExpiredException)
            {
                _messenger.Send<ShowMessage>(new ShowMessage(MessageState.Warn, "Session expired", "You must log in again", "OK"));
                await _authenticationService.LogoutAsync();
            }

            return;
        }
    }
}
