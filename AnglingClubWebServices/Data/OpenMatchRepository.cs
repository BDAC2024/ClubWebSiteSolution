using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class OpenMatchRepository : RepositoryBase, IOpenMatchRepository
    {
        private const string IdPrefix = "OpenMatch";
        private readonly ILogger<OpenMatchRepository> _logger;

        public OpenMatchRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OpenMatchRepository>();
        }

        public async Task AddOrUpdateOpenMatch(OpenMatch match)
        {
            var client = GetClient();

            if (match.IsNewItem)
            {
                match.DbKey = match.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties 
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Date", Value = dateToString(match.Date), Replace = true },
                new ReplaceableAttribute { Name = "Draw", Value = dateToString(match.Draw), Replace = true },
                new ReplaceableAttribute { Name = "Starts", Value = dateToString(match.Starts), Replace = true },
                new ReplaceableAttribute { Name = "Ends", Value = dateToString(match.Ends), Replace = true },
                new ReplaceableAttribute { Name = "Venue", Value = match.Venue, Replace = true },
                new ReplaceableAttribute { Name = "PegsAvailable", Value = ((int)match.PegsAvailable).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "OpenMatchType", Value = ((int)match.OpenMatchType).ToString(), Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = match.DbKey,
                    Attributes = attributes
                }
            );

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"Open Match added: {match.DbKey} - {match.Date.ToShortDateString()}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        public async Task<List<OpenMatch>> GetOpenMatches()
        {
            _logger.LogWarning($"Getting Open Matches at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var openMatches = new List<OpenMatch>();

            var items = await GetData(IdPrefix, "AND Date > ''", "ORDER BY Date ASC");

            foreach (var item in items)
            {
                var openMatch = new OpenMatch();

                openMatch.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Date":
                            openMatch.Date = DateTime.Parse(attribute.Value);
                            break;

                        case "Draw":
                            openMatch.Draw = DateTime.Parse(attribute.Value);
                            break;

                        case "Starts":
                            openMatch.Starts = DateTime.Parse(attribute.Value);
                            break;

                        case "Ends":
                            openMatch.Ends = DateTime.Parse(attribute.Value);
                            break;

                        case "Venue":
                            openMatch.Venue = attribute.Value;
                            break;

                        case "PegsAvailable":
                            openMatch.PegsAvailable = Convert.ToInt32(attribute.Value);
                            break;

                        case "OpenMatchType":
                            openMatch.OpenMatchType = (OpenMatchType)(Convert.ToInt32(attribute.Value));
                            break;

                        default:
                            break;
                    }
                }

                openMatches.Add(openMatch);
            }

            return openMatches;

        }

        public async Task DeleteOpenMatch(string id)
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
    }
}
