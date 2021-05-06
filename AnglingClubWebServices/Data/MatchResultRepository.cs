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
    public class MatchResultRepository : RepositoryBase, IMatchResultRepository
    {
        private const string IdPrefix = "MatchResult";
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MatchResultRepository> _logger;

        public MatchResultRepository(
            IOptions<RepositoryOptions> opts,
            IEventRepository eventRepository,
            IMemberRepository memberRepository,
            ILoggerFactory loggerFactory) : base(opts.Value.AWSAccessId, opts.Value.AWSSecret, opts.Value.SimpleDbDomain, loggerFactory)
        {
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _logger = loggerFactory.CreateLogger<MatchResultRepository>();
        }

        public async Task AddOrUpdateMatchResult(MatchResult result)
        {
            var client = GetClient();

            // Validate MatchId
            {
                if (!(await _eventRepository.GetEvents()).Any(x => x.Id == result.MatchId))
                {
                    throw new KeyNotFoundException($"Match '{result.MatchId}' does not exist");
                }
            }

            // Validate MembershipNumber
            {
                if (!(await _memberRepository.GetMembers()).Any(x => x.MembershipNumber == result.MembershipNumber))
                {
                    throw new KeyNotFoundException($"Member '{result.MembershipNumber}' does not exist");
                }
            }

            if (result.DbKey == null)
            {
                result.DbKey = result.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "MatchId", Value = result.MatchId, Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = result.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Peg", Value = result.Peg, Replace = true },
                new ReplaceableAttribute { Name = "WeightDecimal", Value = weightToString(result.WeightDecimal), Replace = true },
                new ReplaceableAttribute { Name = "Points", Value = pointsToString(result.Points), Replace = true }

            };


            request.Items.Add(
                new ReplaceableItem
                {
                    Name = result.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                _logger.LogDebug($"Match result added");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<MatchResult>> GetMatchResults(string matchId)
        {
            _logger.LogWarning($"Getting matches for : {matchId}");

            var results = new List<MatchResult>();

            var client = GetClient();

            SelectRequest request = new SelectRequest();
            request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{IdPrefix}:%' AND MatchId = '{matchId}' AND WeightDecimal > '' ORDER BY WeightDecimal DESC";

            SelectResponse response = await client.SelectAsync(request);

            foreach (var item in response.Items)
            {
                var result = new MatchResult();

                result.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "MatchId":
                            result.MatchId = attribute.Value;
                            break;

                        case "MembershipNumber":
                            result.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "Peg":
                            result.Peg = attribute.Value;
                            break;

                        case "WeightDecimal":
                            result.WeightDecimal = float.Parse(attribute.Value);
                            break;

                        case "Points":
                            result.Points = float.Parse(attribute.Value);
                            break;

                        default:
                            break;
                    }
                }

                results.Add(result);
            }

            return results;

        }


    }
}
