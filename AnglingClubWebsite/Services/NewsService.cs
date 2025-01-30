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
        private readonly IMessenger _messenger;

        public NewsService(
            IHttpClientFactory httpClientFactory,
            ILogger<NewsService> logger,
            IMessenger messenger) : base(CONTROLLER, httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
        }


        public async Task<List<NewsItem>?> ReadNews()
        {
            _messenger.Send(new ShowProgress());

            _logger.LogInformation($"ReadNews: Accessing {Http.BaseAddress}{Constants.API_NEWS_READ}");

            var response = await Http.GetAsync(Constants.API_NEWS_READ);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"ReadNews: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                _messenger.Send(new HideProgress());
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
                finally
                {
                    _messenger.Send(new HideProgress());
                }
            }


        }
    }
}
