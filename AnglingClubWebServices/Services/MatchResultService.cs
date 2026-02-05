using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
//using MatchType = AnglingClubWebServices.Interfaces.MatchType;

namespace AnglingClubWebServices.Services
{
    public class MatchResultService : IMatchResultService
    {
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MatchResultService> _logger;
        private readonly ITrophyWinnerRepository _trophyWinnerRepository;

        public MatchResultService(
            IMatchResultRepository matchResultRepository,
            IEventRepository eventRepository,
            IMemberRepository memberRepository,
            ILoggerFactory loggerFactory
,
            ITrophyWinnerRepository trophyWinnerRepository)
        {
            _matchResultRepository = matchResultRepository;
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _logger = loggerFactory.CreateLogger<MatchResultService>();
            _trophyWinnerRepository = trophyWinnerRepository;
        }

        public List<MatchResult> GetMemberResults(List<string> matchIds, int membershipNumber)
        {
            var results = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId) && x.MembershipNumber == membershipNumber).OrderBy(x => x.MatchId).ToList();

            return results;
        }

        public List<MatchResult> GetResults(string matchId, ClubEvent match)
        {
            var results = (_matchResultRepository.GetMatchResults(matchId).Result).ToList();

            if (match.AggregateType == AggregateType.PairsPointsAsc)
            {
                results = results.OrderBy(r => r.Points).ThenBy(r => r.WeightDecimal).ToList();
            }
            else
            {
                results = results.OrderByDescending(r => r.Points).ThenByDescending(r => r.WeightDecimal).ToList();
            }

            var pos = 1;
            int numberAtPos = 0;

            float lastWeight = results.Any() ? results.Max(r => r.WeightDecimal) : 0f;
            float lastPoints = match.AggregateType == AggregateType.PairsPointsAsc ? -10000 : 10000;

            foreach (var result in results)
            {

                if (match.MatchType == MatchType.OSU)
                {
                    if (result.Points < lastPoints)
                    {
                        pos += numberAtPos;
                        lastPoints = result.Points;
                        numberAtPos = 0;

                    }
                    if (result.Points == lastPoints)
                    {
                        numberAtPos++;
                    }
                }
                else if (match.AggregateType == AggregateType.PairsPointsAsc)
                {
                    if (result.Points > lastPoints)
                    {
                        pos += numberAtPos;
                        lastPoints = result.Points;
                        numberAtPos = 0;
                    }
                    if (result.Points == lastPoints)
                    {
                        numberAtPos++;
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

                    if (result.WeightDecimal == lastWeight)
                    {
                        numberAtPos++;
                    }
                }

                result.Position = match.MatchType == MatchType.OSU || match.AggregateType == AggregateType.PairsPointsAsc || result.WeightDecimal > 0 ? pos : 0;
            }

            return results;
        }

        public List<LeaguePosition> GetLeagueStandings(AggregateType aggType, Season season)
        {
            var matchIds = _eventRepository.GetEvents().Result.Where(x => x.AggregateType == aggType && x.Season == season).Select(x => x.Id);
            var matchResultsForLeague = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId));
            var members = _memberRepository.GetMembers(season, true).Result;
            var memberLookup = members.ToDictionary(m => m.MembershipNumber);

            var distinctMatchMembers = matchResultsForLeague.DistinctBy(x => x.MembershipNumber).Select(x => x.MembershipNumber);

            foreach (var member in distinctMatchMembers)
            {
                if (!memberLookup.ContainsKey(member))
                {
                    _logger.LogWarning($"GetLeagueStandings: Cannot find member {member}");
                }
            }

            var league = matchResultsForLeague
                .GroupBy(x => x.MembershipNumber)
                .Where(cl => memberLookup.ContainsKey(cl.Key))
                .Select(cl =>
                {
                    var member = memberLookup[cl.Key];

                    return new LeaguePosition
                    {
                        MembershipNumber = cl.Key,
                        Name = member.Name,
                        Points = cl.Sum(c => c.Points),
                        TotalWeightDecimal = cl.Sum(c => c.WeightDecimal)
                    };

                }).ToList();

            var pos = 1;
            int numberAtPos = 0;

            if (aggType == AggregateType.PairsPointsAsc)
            {
                float lastPoints = league.Any() ? league.Min(r => r.Points) : 0f;
                float lastWeight = league.Any() ? league.Max(r => r.TotalWeightDecimal) : 0f;

                foreach (var member in league.OrderBy(x => x.Points).ThenByDescending(x => x.TotalWeightDecimal))
                {
                    if (member.Points > lastPoints)
                    {
                        pos += numberAtPos;
                        lastPoints = member.Points;
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

                        numberAtPos++;
                    }

                    member.Position = pos;
                }

                return league.OrderBy(x => x.Position).ToList();
            }
            else
            {
                float lastPoints = league.Any() ? league.Max(r => r.Points) : 0f;

                foreach (var member in league.OrderByDescending(x => x.Points).ThenByDescending(x => x.TotalWeightDecimal))
                {
                    if (member.Points < lastPoints)
                    {
                        pos += numberAtPos;
                        lastPoints = member.Points;
                        numberAtPos = 0;
                    }

                    if (member.Points == lastPoints)
                    {
                        numberAtPos++;
                    }

                    member.Position = pos;
                }

                return league.OrderByDescending(x => x.Points).ToList();
            }

        }

        public List<AggregateWeight> GetAggregateWeights(AggregateType aggType, Season season)
        {
            var matchIds = _eventRepository.GetEvents().Result.Where(x => x.AggregateType == aggType && x.Season == season).Select(x => x.Id);
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

        public List<TrophyWinner> GetTrophyWinners(TrophyType trophyType, Season season)
        {
            List<int> aggregateTypes = new List<int>();

            if (trophyType == TrophyType.Senior)
            {
                aggregateTypes.Add((int)AggregateType.ClubRiver);
                aggregateTypes.Add((int)AggregateType.ClubPond);
                aggregateTypes.Add((int)AggregateType.Pairs);
                aggregateTypes.Add((int)AggregateType.None);
            }
            else
            {
                // Not needed because Junior winners are explicityly entered in Trophy table, not taken from match results
                //aggregateTypes.Add((int)AggregateType.Junior);
            }

            var matchesWithTrophies = _eventRepository.GetEvents().Result.Where(x => x.EventType == EventType.Match && x.AggregateType.HasValue && aggregateTypes.Contains((int)x.AggregateType.Value) && x.Season == season && x.Cup != null && x.Cup != "" && x.Cup != "Officials");
            var matchIds = matchesWithTrophies.Select(x => x.Id);
            var matchResultsForTrophies = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId));
            var members = _memberRepository.GetMembers(season, true).Result;

            List<TrophyWinner> trophyWinners = new List<TrophyWinner>();

            foreach (ClubEvent match in matchesWithTrophies)
            {
                var trophyWinner = new TrophyWinner()
                {
                    AggregateType = match.AggregateType,
                    Date = match.Date,
                    DateDesc = "",
                    MatchType = match.MatchType,
                    Season = match.Season,
                    Trophy = match.Cup,
                    TrophyType = trophyType,
                    Venue = match.Description
                };

                if (matchResultsForTrophies.Any(x => x.MatchId == match.Id))
                {
                    var winningWeight = matchResultsForTrophies.Where(x => x.MatchId == match.Id).OrderByDescending(x => x.WeightDecimal).First().WeightDecimal;
                    var winners = matchResultsForTrophies.Where(x => x.MatchId == match.Id && x.WeightDecimal == winningWeight);

                    trophyWinner.WeightDecimal = winningWeight;
                    trophyWinner.Winner = "";
                    foreach (var winner in winners)
                    {
                        if (!string.IsNullOrEmpty(trophyWinner.Winner))
                        {
                            trophyWinner.Winner += "/";
                        }
                        trophyWinner.Winner += members.Single(x => x.MembershipNumber == winner.MembershipNumber).Name;
                    }
                    trophyWinner.IsRunning = false;
                }
                else
                {
                    trophyWinner.IsRunning = true;
                }

                trophyWinners.Add(trophyWinner);
            }


            var nonMatchtrophyWinners = (_trophyWinnerRepository.GetTrophyWinners().Result).Where(x => x.Season == season && x.TrophyType == trophyType);

            trophyWinners.AddRange(nonMatchtrophyWinners);

            // Set to "Final" if the trophy match has a date, has a winner and is before today
            foreach (var winner in trophyWinners.Where(x => x.Date.HasValue && x.Winner != "" && x.Date < DateTime.Now))
            {
                winner.IsRunning = false;
            }

            return trophyWinners.OrderBy(x => x.DateDesc).ThenBy(x => x.Date).ToList();
        }
    }

}
