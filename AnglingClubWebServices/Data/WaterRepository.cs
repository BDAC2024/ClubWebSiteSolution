using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
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

            if (water.DbKey == null)
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

                new ReplaceableAttribute { Name = "Icon", Value = water.Icon, Replace = true },
                new ReplaceableAttribute { Name = "Label", Value = water.Label, Replace = true },

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


    }
}
