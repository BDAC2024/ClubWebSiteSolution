using AnglingClubShared;
using AnglingClubShared.Entities;
using AnglingClubShared.Models.Auth;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;

namespace AnglingClubWebsite.Services
{
    public class NewsService : DataServiceBase, INewsService
    {
        private static string CONTROLLER = "News";

        private readonly ILogger<NewsService> _logger;

        public NewsService(
            IHttpClientFactory httpClientFactory,
            ILogger<NewsService> logger) : base(httpClientFactory)
        {
            _logger = logger;
        }


        public async Task<List<NewsItem>?> ReadNews()
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_NEWS}";

            _logger.LogInformation($"ReadNews: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"ReadNews: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else 
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<List<NewsItem>>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ReadNews: {ex.Message}");
                    throw;
                }
            }
        }


        public async Task DeleteNewsItem(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_NEWS}/{id}";

            _logger.LogInformation($"DeleteNewsItem: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.DeleteAsync($"{relativeEndpoint}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"DeleteNewsItem: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
            }

            return;
        }
    }
}
