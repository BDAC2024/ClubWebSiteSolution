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
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MatchResultService> _logger;

        public MatchResultService(
            IMatchResultRepository matchResultRepository,
            IEventRepository eventRepository,
            IMemberRepository memberRepository,
            ILoggerFactory loggerFactory
        )
        {
            _matchResultRepository = matchResultRepository;
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _logger = loggerFactory.CreateLogger<MatchResultService>();
        }

        public List<MatchResult> GetResults(string matchId, MatchType matchType)
        {
            var results = (_matchResultRepository.GetMatchResults(matchId).Result).OrderByDescending(r => r.Points).ThenByDescending(r => r.WeightDecimal).ToList();

            var pos = 1;
            int numberAtPos = 0;

            float lastWeight = results.Any() ? results.Max(r => r.WeightDecimal) : 0f;
            float lastPoints = 10000;

            foreach (var result in results)
            {

                if (matchType == MatchType.OSU)
                {
                    if (result.Points < lastPoints)
                    {
                        pos += numberAtPos;
                        lastPoints = result.Points;
                        numberAtPos = 0;

                    }
                }
                else
                {
                    if (result.WeightDecimal < lastWeight)
                    {
                        pos += numberAtPos;
                        lastWeight = result.WeightDecimal;
                        numberAtPos = 0;
                    }

                }


                if (result.WeightDecimal == lastWeight)
                {
                    numberAtPos++;
                }

                result.Position = matchType == MatchType.OSU || result.WeightDecimal > 0 ? pos : 0;
            }

            return results;
        }

        public List<LeaguePosition> GetLeagueStandings(MatchType matchType, Season season)
        {
            var matchIds = _eventRepository.GetEvents().Result.Where(x => x.MatchType == matchType && x.Season == season).Select(x => x.Id);
            var matchResultsForLeague = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId));
            var members = _memberRepository.GetMembers(season, true).Result;

            var league = matchResultsForLeague
                .GroupBy(x => x.MembershipNumber)
                .Select(cl => new LeaguePosition
                {
                    MembershipNumber = members.Single(x => x.MembershipNumber == cl.First().MembershipNumber).MembershipNumber,
                    Name = members.Single(x => x.MembershipNumber == cl.First().MembershipNumber).Name,
                    Points = cl.Sum(c => c.Points),
                    TotalWeightDecimal = cl.Sum(x=> x.WeightDecimal)
                }).ToList();

            var pos = 1;
            int numberAtPos = 0;

            float lastPoints = league.Any() ? league.Max(r => r.Points) : 0f;
            float lastWeight = league.Any() ? league.First(x => x.Points == lastPoints).TotalWeightDecimal : 0f;

            foreach (var member in league.OrderByDescending(x => x.Points).ThenByDescending(x => x.TotalWeightDecimal))
            {
                if (member.Points < lastPoints)
                {
                    pos += numberAtPos;
                    lastPoints = member.Points;
                    lastWeight = member.TotalWeightDecimal;
                    numberAtPos = 0;
                }

                if (member.Points == lastPoints)
                {
                    if (member.TotalWeightDecimal < lastWeight)
                    {
                        pos += numberAtPos;
                        lastWeight = member.TotalWeightDecimal;
                        numberAtPos = 0;
                    }

                    if (member.TotalWeightDecimal == lastWeight)
                    {
                        numberAtPos++;
                    }
                    
                }

                member.Position = pos;
            }

            return league.OrderByDescending(x => x.Points).ThenByDescending(x => x.TotalWeightDecimal).ToList();
        }

        public List<AggregateWeight> GetAggregateWeights(AggregateWeightType aggWeightType, Season season)
        {
            var matchIds = _eventRepository.GetEvents().Result.Where(x => x.AggregateWeightType == aggWeightType && x.Season == season).Select(x => x.Id);
            var matchResultsForLeague = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId));
            var members = _memberRepository.GetMembers(season, true).Result;

            var league = matchResultsForLeague
                .GroupBy(x => x.MembershipNumber)
                .Select(cl => new AggregateWeight
                {
                    Name = members.Single(x => x.MembershipNumber == cl.First().MembershipNumber).Name,
                    TotalWeightDecimal = cl.Sum(x => x.WeightDecimal),
                    MembershipNumber = members.Single(x => x.MembershipNumber == cl.First().MembershipNumber).MembershipNumber
                }).ToList();

            var pos = 1;
            int numberAtPos = 0;

            float lastWeight = league.Any() ? league.Max(r => r.TotalWeightDecimal) : 0f;

            foreach (var member in league.OrderByDescending(x => x.TotalWeightDecimal))
            {
                if (member.TotalWeightDecimal < lastWeight)
                {
                    pos += numberAtPos;
                    lastWeight = member.TotalWeightDecimal;
                    numberAtPos = 0;
                }

                if (member.TotalWeightDecimal == lastWeight)
                {
                    numberAtPos++;
                }

                member.Position = pos;
            }

            return league.OrderByDescending(x => x.TotalWeightDecimal).ToList();
        }
    }
}
