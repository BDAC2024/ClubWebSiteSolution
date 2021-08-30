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
        
        private const string MultiValueSeparator = "~|";
        private const int MultiValueSegmentSize = 1000;

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
                new ReplaceableAttribute { Name = "Species", Value = water.Species, Replace = true },

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

            await UpdateDesc(water);
            await UpdateDirections(water);
        }

        public async Task UpdateDesc(Water water)
        {
            for (int i = 0; i < (water.Description.Length / MultiValueSegmentSize) + 1; i++)
            {
                await AddPartOfDescription(water.DbKey, water.Description, i);
            }

        }

        public async Task UpdateDirections(Water water)
        {
            for (int i = 0; i < (water.Directions.Length / MultiValueSegmentSize) + 1; i++)
            {
                await AddPartOfDirections(water.DbKey, water.Directions, i);
            }

        }


        private async Task AddPartOfDescription(string dbKey, string description, int index)
        {
            var descriptionSegment = new string(description.Skip(index * MultiValueSegmentSize).Take(MultiValueSegmentSize).ToArray());

            if (descriptionSegment != "")
            {
                var client = GetClient();

                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                // Mandatory Properties
                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Description", Value = $"{index}{MultiValueSeparator}{descriptionSegment}", Replace = index == 0 },
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
                    BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                    _logger.LogDebug($"Water description segment added");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
        }

        private async Task AddPartOfDirections(string dbKey, string directions, int index)
        {
            var directionsSegment = new string(directions.Skip(index * MultiValueSegmentSize).Take(MultiValueSegmentSize).ToArray());

            if (directionsSegment != "")
            {
                var client = GetClient();

                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                // Mandatory Properties
                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Directions", Value = $"{index}{MultiValueSeparator}{directionsSegment}", Replace = index == 0 },
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
                    BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                    _logger.LogDebug($"Water directions segment added");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
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
                var descriptionArr = new List<MultiValued>();
                var directionArr = new List<MultiValued>();

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
                            descriptionArr.Add(GetMultiValuedElement(attribute.Value));
                            break;

                        case "Species":
                            water.Species = attribute.Value;
                            break;

                        case "Directions":
                            directionArr.Add(GetMultiValuedElement(attribute.Value));
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

                water.Description = string.Join("", descriptionArr.OrderBy(x => x.Index).Select(x => x.Text).ToArray());
                water.Directions = string.Join("", directionArr.OrderBy(x => x.Index).Select(x => x.Text).ToArray());

                waters.Add(water);
            }

            return waters.OrderBy(x => x.Id).ToList();

        }

        private MultiValued GetMultiValuedElement(string attributeValue)
        {
            if (attributeValue.Contains(MultiValueSeparator))
            {
                return new MultiValued { Index = Convert.ToInt32(attributeValue.Split(MultiValueSeparator)[0]), Text = attributeValue.Split(MultiValueSeparator)[1] };
            }
            else
            {
                return new MultiValued { Index = 0, Text = attributeValue };
            }
        }

        private class MultiValued
        {
            public int Index { get; set; }
            public string Text { get; set; }
        }
    }
}
