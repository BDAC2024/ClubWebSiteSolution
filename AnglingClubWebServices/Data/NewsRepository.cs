using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class NewsRepository : RepositoryBase, INewsRepository
    {
        private const string IdPrefix = "NewsItem";
        private readonly ILogger<NewsRepository> _logger;

        public NewsRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<NewsRepository>();
        }

        public async Task AddOrUpdateNewsItem(NewsItem newsItem)
        {
            var client = GetClient();

            if (newsItem.IsNewItem)
            {
                newsItem.DbKey = newsItem.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Date", Value = dateToString(newsItem.Date), Replace = true },
                new ReplaceableAttribute { Name = "Title", Value = newsItem.Title, Replace = true },
                //new ReplaceableAttribute { Name = "Body", Value = newsItem.Body, Replace = true },

            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = newsItem.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"News Item added: {newsItem.DbKey} - {newsItem.Title}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

            await UpdateBody(newsItem);

        }

        public async Task<List<NewsItem>> GetNewsItems()
        {
            _logger.LogWarning($"Getting news items at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var newsItems = new List<NewsItem>();

            var items = await GetData(IdPrefix, "AND Date > ''", "ORDER BY Date DESC");

            foreach (var item in items)
            {
                var bodyArr = new List<MultiValued>();

                var newsItem = new NewsItem();

                newsItem.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Date":
                            newsItem.Date = DateTime.Parse(attribute.Value);
                            break;


                        case "Title":
                            newsItem.Title = attribute.Value;
                            break;

                        case "Body":
                            bodyArr.Add(GetMultiValuedElement(attribute.Value));
                            //newsItem.Body = attribute.Value;
                            break;


                        default:
                            break;
                    }
                }

                newsItem.Body = string.Join("", bodyArr.OrderBy(x => x.Index).Select(x => x.Text).ToArray());

                newsItems.Add(newsItem);
            }

            return newsItems;

        }

        public async Task DeleteNewsItem(string id)
        {
            var client = GetClient();

            DeleteAttributesRequest request = new DeleteAttributesRequest();

            //request.Attributes.Add(new Amazon.SimpleDB.Model.Attribute { Name = id });
            request.DomainName = Domain;
            request.ItemName = id;

            try
            {
                DeleteAttributesResponse response = await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }


        }

        private async Task UpdateBody(NewsItem newsItem)
        {
            for (int i = 0; i < (newsItem.Body.Length / MultiValueSegmentSize) + 1; i++)
            {
                await AddPartOfBody(newsItem.DbKey, newsItem.Body, i);
            }

        }

        private async Task AddPartOfBody(string dbKey, string body, int index)
        {
            var bodySegment = new string(body.Skip(index * MultiValueSegmentSize).Take(MultiValueSegmentSize).ToArray());

            if (bodySegment != "")
            {
                var client = GetClient();

                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                // Mandatory Properties
                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Body", Value = $"{index}{MultiValueSeparator}{bodySegment}", Replace = index == 0 },
                };

                request.Items.Add(
                    new ReplaceableItem
                    {
                        Name = dbKey,
                        Attributes = attributes
                    }
                );

                try
                {
                    //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                    await WriteInBatches(request, client);
                    _logger.LogDebug($"News body segment added");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
        }

    }
}
