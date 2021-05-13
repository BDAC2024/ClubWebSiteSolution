using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class WaterRepository : RepositoryBase, IWaterRepository
    {
        private const string IdPrefix = "Water";
        private readonly ILogger<WaterRepository> _logger;

        public WaterRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value.AWSAccessId, opts.Value.AWSSecret, opts.Value.SimpleDbDomain, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WaterRepository>();
        }

        public async Task AddOrUpdateWater(Water water)
        {
            var client = GetClient();

            if (water.IsNewItem)
            {
                water.DbKey = water.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory Properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Id", Value = water.Id.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Name", Value = water.Name, Replace = true },
                new ReplaceableAttribute { Name = "Type", Value = ((int)water.Type).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Access", Value = ((int)water.Access).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Description", Value = water.Description, Replace = true },
                new ReplaceableAttribute { Name = "Species", Value = water.Species, Replace = true },
                new ReplaceableAttribute { Name = "Directions", Value = water.Directions, Replace = true },

                new ReplaceableAttribute { Name = "Markers", Value = water.Markers, Replace = true },
                new ReplaceableAttribute { Name = "MarkerIcons", Value = water.MarkerIcons, Replace = true },
                new ReplaceableAttribute { Name = "MarkerLabels", Value = water.MarkerLabels, Replace = true },

                new ReplaceableAttribute { Name = "Destination", Value = water.Destination, Replace = true },
                new ReplaceableAttribute { Name = "Path", Value = water.Path, Replace = true },

            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = water.DbKey,
                    Attributes = attributes
                }
            );

            try
            {
                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                _logger.LogDebug($"Water added");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Water>> GetWaters()
        {
            _logger.LogWarning($"Getting waters at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var waters = new List<Water>();

            var client = GetClient();

            SelectRequest request = new SelectRequest();
            request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{IdPrefix}:%' AND Name > '' ORDER BY Name";

            SelectResponse response = await client.SelectAsync(request);

            foreach (var item in response.Items)
            {
                var water = new Water();

                water.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Id":
                            water.Id = float.Parse(attribute.Value);
                            break;

                        case "Name":
                            water.Name = attribute.Value;
                            break;

                        case "Type":
                            water.Type = (WaterType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Access":
                            water.Access = (WaterAccessType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Description":
                            water.Description = attribute.Value;
                            break;

                        case "Species":
                            water.Species = attribute.Value;
                            break;

                        case "Directions":
                            water.Directions = attribute.Value;
                            break;

                        case "Markers":
                            water.Markers = attribute.Value;
                            break;

                        case "MarkerIcons":
                            water.MarkerIcons = attribute.Value;
                            break;

                        case "MarkerLabels":
                            water.MarkerLabels = attribute.Value;
                            break;

                        case "Destination":
                            water.Destination = attribute.Value;
                            break;

                        case "Path":
                            water.Path = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                waters.Add(water);
            }

            return waters.OrderBy(x => x.Id).ToList();

        }

    }
}
