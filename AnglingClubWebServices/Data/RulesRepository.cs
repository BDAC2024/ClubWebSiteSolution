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
    public class RulesRepository : RepositoryBase, IRulesRepository
    {
        private const string IdPrefix = "Rules";
        private readonly ILogger<RulesRepository> _logger;

        public RulesRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value.AWSAccessId, opts.Value.AWSSecret, opts.Value.SimpleDbDomain, loggerFactory)
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
                new ReplaceableAttribute { Name = "Body", Value = rules.Body, Replace = true },
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
                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                _logger.LogDebug($"Rules added: {rules.DbKey} - {rules.Title}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Rules>> GetRules()
        {
            _logger.LogWarning($"Getting rules at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var rules = new List<Rules>();

            var client = GetClient();

            SelectRequest request = new SelectRequest();
            request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{IdPrefix}:%'";

            SelectResponse response = await client.SelectAsync(request);

            foreach (var item in response.Items)
            {
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
                            rule.Body = attribute.Value;
                            break;

                        case "RuleType":
                            rule.RuleType = (RuleType)(Convert.ToInt32(attribute.Value));
                            break;

                        default:
                            break;
                    }
                }

                rules.Add(rule);
            }

            return rules;

        }

    }
}
