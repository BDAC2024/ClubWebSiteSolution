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
    public class TrophyWinnerRepository : RepositoryBase, ITrophyWinnerRepository
    {
        private const string IdPrefix = "TrophyWinner";
        private readonly ILogger<TrophyWinnerRepository> _logger;

        public TrophyWinnerRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TrophyWinnerRepository>();
        }

        public async Task AddOrUpdateTrophyWinner(TrophyWinner trophyWinner)
        {
            var client = GetClient();

            try
            {

                if (trophyWinner.IsNewItem)
                {
                    trophyWinner.DbKey = trophyWinner.GenerateDbKey(IdPrefix);
                }

                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                // Mandatory properties
                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Trophy", Value = trophyWinner.Trophy, Replace = true },
                    new ReplaceableAttribute { Name = "TrophyType", Value = ((int)trophyWinner.TrophyType).ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "IsRunning", Value = trophyWinner.IsRunning ? "1" : "0", Replace = true },
                    new ReplaceableAttribute { Name = "Winner", Value = trophyWinner.Winner, Replace = true },
                    new ReplaceableAttribute { Name = "WeightDecimal", Value = weightToString(trophyWinner.WeightDecimal), Replace = true },
                    new ReplaceableAttribute { Name = "Points", Value = pointsToString(trophyWinner.Points), Replace = true },
                    new ReplaceableAttribute { Name = "Venue", Value = trophyWinner.Venue, Replace = true },
                    new ReplaceableAttribute { Name = "DateDesc", Value = trophyWinner.DateDesc, Replace = true },
                    new ReplaceableAttribute { Name = "Date", Value = trophyWinner.Date.HasValue ? dateToString(trophyWinner.Date.Value) : "NULL", Replace = true },
                    new ReplaceableAttribute { Name = "Season", Value = ((int)trophyWinner.Season).ToString(), Replace = true },

                };

                // Optional properties
                if (trophyWinner.AggregateType != null) { attributes.Add(new ReplaceableAttribute { Name = "AggregateType", Value = ((int)trophyWinner.AggregateType.Value).ToString(), Replace = true }); }
                if (trophyWinner.MatchType != null) { attributes.Add(new ReplaceableAttribute { Name = "MatchType", Value = ((int)trophyWinner.MatchType.Value).ToString(), Replace = true }); }

                request.Items.Add(
                    new ReplaceableItem
                    {
                        Name = trophyWinner.DbKey,
                        Attributes = attributes
                    }
                );

                try
                {
                    //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                    await WriteInBatches(request, client);
                    _logger.LogDebug($"Trophy Winner added: {trophyWinner.DbKey} - {trophyWinner.Trophy}");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to store Trophy Winner");
                throw;
            }

        }

        public async Task<List<TrophyWinner>> GetTrophyWinners()
        {
            _logger.LogWarning($"Getting trophy winners at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var trophyWinners = new List<TrophyWinner>();

            var items = await GetData(IdPrefix, "AND Date > ''", "ORDER BY Date ASC");

            foreach (var item in items)
            {
                var trophyWinner = new TrophyWinner();

                trophyWinner.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {

                        case "Trophy":
                            trophyWinner.Trophy = attribute.Value;
                            break;

                        case "TrophyType":
                            trophyWinner.TrophyType = (TrophyType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "IsRunning":
                            trophyWinner.IsRunning = attribute.Value == "0" ? false : true;
                            break;

                        case "Winner":
                            trophyWinner.Winner = attribute.Value;
                            break;

                        case "WeightDecimal":
                            trophyWinner.WeightDecimal = float.Parse(attribute.Value);
                            break;

                        case "Points":
                            trophyWinner.Points = float.Parse(attribute.Value);
                            break;

                        case "Venue":
                            trophyWinner.Venue = attribute.Value;
                            break;

                        case "DateDesc":
                            trophyWinner.DateDesc = attribute.Value;
                            break;

                        case "Date":
                            trophyWinner.Date = attribute.Value != "NULL" ? DateTime.Parse(attribute.Value) : null;
                            break;

                        case "Season":
                            trophyWinner.Season = (Season)(Convert.ToInt32(attribute.Value));
                            break;
                            
                        case "MatchType":
                            trophyWinner.MatchType = (MatchType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "AggregateType":
                            trophyWinner.AggregateType = (AggregateType)(Convert.ToInt32(attribute.Value));
                            break;

                        default:
                            break;
                    }
                }

                trophyWinners.Add(trophyWinner);
            }

            return trophyWinners;

        }

    }
}
