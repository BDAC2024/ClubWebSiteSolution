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
    public class RulesRepository : RepositoryBase, IRulesRepository
    {
        private const string IdPrefix = "Rules";
        private readonly ILogger<RulesRepository> _logger;

        public RulesRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RulesRepository>();
        }

        public async Task AddOrUpdateRules(Rules rules)
        {
            var client = GetClient();

            if (rules.IsNewItem)
            {
                rules.DbKey = rules.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Title", Value = rules.Title, Replace = true },
                new ReplaceableAttribute { Name = "RuleType", Value = ((int)rules.RuleType).ToString(), Replace = true },

            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = rules.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"Rules added: {rules.DbKey} - {rules.Title}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

            await UpdateBody(rules);
        }

        public async Task UpdateBody(Rules rules)
        {
            for (int i = 0; i < (rules.Body.Length / MultiValueSegmentSize) + 1; i++)
            {
                await AddPartOfBody(rules.DbKey, rules.Body, i);
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
                    _logger.LogDebug($"Rules body segment added");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
        }

        public async Task<List<Rules>> GetRules()
        {
            _logger.LogWarning($"Getting rules at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var rules = new List<Rules>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var bodyArr = new List<MultiValued>();

                var rule = new Rules();

                rule.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Title":
                            rule.Title = attribute.Value;
                            break;

                        case "Body":
                            bodyArr.Add(GetMultiValuedElement(attribute.Value));
                            break;

                        case "RuleType":
                            rule.RuleType = (RuleType)(Convert.ToInt32(attribute.Value));
                            break;

                        default:
                            break;
                    }
                }

                rule.Body = string.Join("", bodyArr.OrderBy(x => x.Index).Select(x => x.Text).ToArray());

                rules.Add(rule);
            }

            return rules;

        }

    }
}
