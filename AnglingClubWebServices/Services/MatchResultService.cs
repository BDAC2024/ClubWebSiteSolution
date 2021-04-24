using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Services
{
    public class MatchResultService : IMatchResultService
    {
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly ILogger<MatchResultService> _logger;

        public MatchResultService(
            IMatchResultRepository matchResultRepository,
            ILoggerFactory loggerFactory
        )
        {
            _matchResultRepository = matchResultRepository;
            _logger = loggerFactory.CreateLogger<MatchResultService>();
        }

        public List<MatchResult> GetResults(string matchId)
        {
            var results = _matchResultRepository.GetMatchResults(matchId).Result;

            var pos = 1;
            int numberAtPos = 0;

            float lastWeight = results.Any() ? results.Max(r => r.WeightDecimal) : 0f;


            foreach (var result in results.OrderByDescending(r => r.WeightDecimal))
            {
                if (result.WeightDecimal < lastWeight)
                {
                    pos+= numberAtPos;
                    lastWeight = result.WeightDecimal;
                    numberAtPos = 0;
                }

                if (result.WeightDecimal == lastWeight)
                {
                    numberAtPos++;
                }

                result.Position = result.WeightDecimal > 0 ? pos : 0;
            }

            return results;
        }
    }
}
