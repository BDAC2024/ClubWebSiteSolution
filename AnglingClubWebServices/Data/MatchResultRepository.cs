using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
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

        private List<ClubEvent> _cachedEvents = null;
        private List<Member> _cachedMembers = null;
        private List<MatchResult> _cachedResults = null;

        public MatchResultRepository(
            IOptions<RepositoryOptions> opts,
            IEventRepository eventRepository,
            IMemberRepository memberRepository,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
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
                _cachedEvents ??= await _eventRepository.GetEvents();
                if (!_cachedEvents.Any(x => x.Id == result.MatchId))
                {
                    throw new KeyNotFoundException($"Match '{result.MatchId}' does not exist");
                }
            }

            // Validate MembershipNumber
            {
                _cachedMembers ??= await _memberRepository.GetMembers();
                if (!_cachedMembers.Any(x => x.MembershipNumber == result.MembershipNumber))
                {
                    throw new KeyNotFoundException($"Member '{result.MembershipNumber}' does not exist");
                }
            }

            // If this result already exists, grab its key
            {
                _cachedResults ??= await GetAllMatchResults();
                var existingResult = _cachedResults.SingleOrDefault(x => x.MatchId == result.MatchId && x.MembershipNumber == result.MembershipNumber);
                if (existingResult != null)
                {
                    result.DbKey = existingResult.DbKey;
                }
            }

            // If key is still blank then generate a new one
            if (result.IsNewItem)
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
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
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
            _logger.LogWarning($"Getting all match results for : {matchId}");

            return (await GetAllMatchResults()).Where(x => x.MatchId == matchId).ToList();

        }

        public async Task<List<MatchResult>> GetAllMatchResults()
        {
            _logger.LogWarning($"Getting all match results");

            var results = new List<MatchResult>();

            var items = await GetData(IdPrefix, "AND WeightDecimal > ''", "ORDER BY WeightDecimal DESC");

            foreach (var item in items)
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
