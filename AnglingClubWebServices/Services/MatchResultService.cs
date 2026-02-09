using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.Models;
using AnglingClubShared.Services;
using AnglingClubWebServices.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var matchesInSeason = matchIds.Count();
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
                    var points = 0f;
                    var fishedMatches = 0;
                    var droppedPoints = 0f;
                    var droppedMatches = 0;

                    var matchesToDrop = MatchHelperService.MatchesToBeDropped(aggType, season);

                    if (matchesToDrop > 0)
                    {
                        var sortedPoints = cl.OrderByDescending(c => c.Points);

                        points = sortedPoints.Take(matchesInSeason - matchesToDrop).Sum(c => c.Points);
                        fishedMatches = cl.Count(); ;
                        droppedPoints = sortedPoints.Skip(matchesInSeason - matchesToDrop).Sum(c => c.Points);
                        droppedMatches = sortedPoints.Skip(matchesInSeason - matchesToDrop).Count();

                    }
                    else
                    {
                        points = cl.Sum(c => c.Points);
                    }

                    return new LeaguePosition
                    {
                        MembershipNumber = cl.Key,
                        Name = member.Name,
                        Points = points,
                        TotalWeightDecimal = cl.Sum(c => c.WeightDecimal),

                        MatchesInSeason = matchesToDrop > 0 ? matchesInSeason : 0,
                        DroppedPoints = droppedPoints,
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
            var matchesInSeason = matchIds.Count();
            var matchResultsForLeague = _matchResultRepository.GetAllMatchResults().Result.Where(x => matchIds.Contains(x.MatchId));
            var members = _memberRepository.GetMembers(season, true).Result;
            var memberLookup = members.ToDictionary(m => m.MembershipNumber);

            var distinctMatchMembers = matchResultsForLeague.DistinctBy(x => x.MembershipNumber).Select(x => x.MembershipNumber);

            foreach (var member in distinctMatchMembers)
            {
                if (!memberLookup.ContainsKey(member))
                {
                    _logger.LogWarning($"GetAggregateWeights: Cannot find member {member}");
                }
            }

            var league = matchResultsForLeague
                .GroupBy(x => x.MembershipNumber)
                .Where(cl => memberLookup.ContainsKey(cl.Key))
                .Select(cl =>
                {
                    var member = memberLookup[cl.Key];

                    var weight = 0f;
                    var fishedMatches = 0;
                    var droppedWeight = 0f;
                    var droppedMatches = 0;

                    var matchesToDrop = MatchHelperService.MatchesToBeDropped(aggType, season);

                    if (matchesToDrop > 0)
                    {
                        var sortedWeights = cl.OrderByDescending(c => c.WeightDecimal);

                        weight = sortedWeights.Take(matchesInSeason - matchesToDrop).Sum(c => c.WeightDecimal);
                        fishedMatches = cl.Count();
                        droppedWeight = sortedWeights.Skip(matchesInSeason - matchesToDrop).Sum(c => c.WeightDecimal);
                        droppedMatches = sortedWeights.Skip(matchesInSeason - matchesToDrop).Count();

                    }
                    else
                    {
                        weight = cl.Sum(c => c.WeightDecimal);
                    }

                    return new AggregateWeight
                    {
                        MembershipNumber = cl.Key,
                        Name = member.Name,
                        TotalWeightDecimal = weight,

                        MatchesInSeason = matchesToDrop > 0 ? matchesInSeason : 0,
                        DroppedWeightDecimal = droppedWeight,

                    };
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
                    if (winningWeight == 0)
                    {
                        trophyWinner.Winner = "No one caught";
                    }
                    else
                    {
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

        public List<MatchAllResultOutputDto> GetResultsForAllMembers()
        {
            var allMatches = _eventRepository.GetEvents().Result.Where(x => x.EventType == EventType.Match);
            var allMembers = _memberRepository.GetMembers().Result;

            var results = getExpandedResults(allMatches, allMembers.AsEnumerable());

            return results;
        }

        public async Task<MemberResultsInSeason> GetMemberResultsInSeason(int membershipNumber, AggregateType aggType, Season season, bool basedOnPoints)
        {
            var results = new MemberResultsInSeason
            {
                MembershipNumber = membershipNumber,
                Season = season,
                AggregateType = aggType
            };

            var relevantMatches = (await _eventRepository.GetEvents()).Where(x => x.EventType == EventType.Match && x.AggregateType == aggType && x.Season == season);
            var members = (await _memberRepository.GetMembers()).Where(x => x.MembershipNumber == membershipNumber && x.SeasonsActive.Contains(season));

            if (relevantMatches.Any() && members.Any())
            {
                results.ResultsCounted = getExpandedResults(relevantMatches, members);

                results.MatchesInSeason = relevantMatches.Count();
                results.MatchesFished = results.ResultsCounted.Count();
                results.MemberName = members.First().Name;

                var matchesToDrop = MatchHelperService.MatchesToBeDropped(aggType, season);

                if (matchesToDrop > 0)
                {
                    if (basedOnPoints)
                    {
                        var sortedResults = results.ResultsCounted.OrderByDescending(r => r.Points).ToList();
                        results.ResultsDropped = sortedResults.Skip(results.MatchesInSeason - matchesToDrop).ToList();
                        results.ResultsCounted = sortedResults.Take(results.MatchesInSeason - matchesToDrop).ToList();
                        results.DroppedPoints = results.ResultsDropped.Sum(r => r.Points);
                        results.MatchesDropped = results.ResultsDropped.Count();
                        results.CountedPoints = results.ResultsCounted.Sum(r => r.Points);
                    }
                    else
                    {
                        var sortedResults = results.ResultsCounted.OrderByDescending(r => r.WeightDecimal).ToList();
                        results.ResultsDropped = sortedResults.Skip(results.MatchesInSeason - matchesToDrop).ToList();
                        results.ResultsCounted = sortedResults.Take(results.MatchesInSeason - matchesToDrop).ToList();
                        results.DroppedWeightDecimal = results.ResultsDropped.Sum(r => r.WeightDecimal);
                        results.MatchesDropped = results.ResultsDropped.Count();
                        results.CountedWeightDecimal = results.ResultsCounted.Sum(r => r.WeightDecimal);
                    }
                }
                else
                {
                    results.CountedPoints = results.ResultsCounted.Sum(r => r.Points);
                    results.CountedWeightDecimal = results.ResultsCounted.Sum(r => r.WeightDecimal);
                }
            }

            return results;
        }


        private List<MatchAllResultOutputDto> getExpandedResults(IEnumerable<ClubEvent> allMatches, IEnumerable<Member> allMembers)
        {
            List<MatchAllResultOutputDto> results = new List<MatchAllResultOutputDto>();

            var allSeasons = allMatches.DistinctBy(x => x.Season).Select(x => x.Season).ToList();
            var allResults = _matchResultRepository.GetAllMatchResults().Result;

            foreach (var season in allSeasons)
            {
                var seasonMembers = allMembers.Where(x => x.SeasonsActive.Contains(season));
                int memberPosition = 0;

                foreach (var match in allMatches.Where(x => x.Season == season))
                {
                    var matchResults = allResults.Where(x => x.MatchId == match.Id);
                    if (allMembers.Count() == 1)
                    {
                        matchResults = matchResults.Where(x => x.MembershipNumber == allMembers.First().MembershipNumber);
                    }
                    List<MatchAllResultOutputDto> matchResultsForMatch = new List<MatchAllResultOutputDto>();

                    foreach (var matchResult in matchResults)
                    {
                        var member = seasonMembers.Single(x => x.MembershipNumber == matchResult.MembershipNumber);
                        var result = new MatchAllResultOutputDto
                        {
                            Name = member.Name,
                            WeightDecimal = matchResult.WeightDecimal,
                            Position = memberPosition,
                            MatchId = matchResult.MatchId,
                            MembershipNumber = matchResult.MembershipNumber,
                            Peg = matchResult.Peg,
                            Points = matchResult.Points,
                            MatchType = match.MatchType.Value.EnumDescription(),
                            AggType = match.AggregateType.Value.EnumDescription(),
                            Season = season.EnumDescription().Split(",")[0],
                            Venue = match.Description,
                            Date = match.Date
                        };
                        matchResultsForMatch.Add(result);
                    }

                    var pos = 1;
                    int numberAtPos = 0;

                    float lastWeight = matchResultsForMatch.Any() ? matchResultsForMatch.Max(r => r.WeightDecimal) : 0f;
                    float lastPoints = 10000;

                    foreach (var result in matchResultsForMatch)
                    {
                        if (match.MatchType == MatchType.OSU)
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

                        result.Position = match.MatchType == MatchType.OSU || result.WeightDecimal > 0 ? pos : 0;
                    }

                    results.AddRange(matchResultsForMatch);
                }
            }


            return results;
        }

    }

}
