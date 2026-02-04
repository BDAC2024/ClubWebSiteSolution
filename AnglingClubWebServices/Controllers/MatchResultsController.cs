using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class MatchResultsController : AnglingClubControllerBase
    {
        private readonly ILogger<MatchResultsController> _logger;
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IMatchResultService _matchResultService;
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMapper _mapper;

        public MatchResultsController(
            IMatchResultRepository matchResultRepository,
            IMatchResultService matchResultService,
            IMemberRepository memberRepository,
            IEventRepository eventRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _matchResultRepository = matchResultRepository;
            _matchResultService = matchResultService;
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<MatchResultsController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet("{matchId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MatchResultOutputDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(string matchId)
        {
            var errors = new List<string>();

            StartTimer();

            var match = _eventRepository.GetEvents().Result.Single(x => x.Id == matchId);
            var results = _mapper.Map<List<MatchResultOutputDto>>(_matchResultService.GetResults(matchId, match));
            var members = _memberRepository.GetMembers(match.Season, true).Result;
            foreach (var result in results)
            {
                var member = members.FirstOrDefault(x => x.MembershipNumber == result.MembershipNumber);
                if (member != null)
                {
                    result.Name = member.Name;
                }
                else
                {
                    result.Name = $"Member {result.MembershipNumber} not found";
                }
            }

            ReportTimer("Getting match results");

            return Ok(results);

        }

        [HttpGet("member/{membershipNumber}/{matchType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MatchResultOutputDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetMemberResults(int membershipNumber, MatchType matchType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            try
            {
                var matchIds = _eventRepository.GetEvents().Result.Where(x => x.MatchType == matchType && x.Season == season).Select(x => x.Id).ToList();
                var results = _mapper.Map<List<MatchResultOutputDto>>(_matchResultService.GetMemberResults(matchIds, membershipNumber));
                var members = _memberRepository.GetMembers(season, true).Result;

                foreach (var result in results)
                {
                    var member = members.FirstOrDefault(x => x.MembershipNumber == result.MembershipNumber);
                    if (member != null)
                    {
                        result.Name = member.Name;
                    }
                    else
                    {
                        result.Name = $"Member {result.MembershipNumber} not found";
                    }
                }

                ReportTimer("Getting match results for member");

                return Ok(results);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }
        }

        [HttpGet("members")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MatchResultOutputDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetAllMembersResults()
        {
            var errors = new List<string>();
            List<MatchAllResultOutputDto> results = new List<MatchAllResultOutputDto>();

            StartTimer();

            try
            {
                var allMatches = _eventRepository.GetEvents().Result.Where(x => x.EventType == EventType.Match).ToList();
                var allSeasons = allMatches.DistinctBy(x => x.Season).Select(x => x.Season).ToList();
                var allMembers = _memberRepository.GetMembers().Result;
                var allResults = _matchResultRepository.GetAllMatchResults().Result;

                foreach (var season in allSeasons)
                {
                    var seasonMembers = allMembers.Where(x => x.SeasonsActive.Contains(season));
                    int memberPosition = 0;

                    foreach (var match in allMatches.Where(x => x.Season == season))
                    {
                        var matchResults = allResults.Where(x => x.MatchId == match.Id);
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
                                Venue = match.Description
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

                ReportTimer("Getting all match results for all members");

                return Ok(results);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }
        }

        [HttpGet("standings/{aggType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeaguePosition>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetLeagueStandings(AggregateType aggType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            try
            {
                var standings = _matchResultService.GetLeagueStandings(aggType, season);

                ReportTimer($"Getting league standings for {aggType} in {season}");

                return Ok(standings);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }
        }

        [HttpGet("aggregateWeights/{aggType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AggregateWeight>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetAggregateWeights(AggregateType aggType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            try
            {
                var standings = _matchResultService.GetAggregateWeights(aggType, season);

                ReportTimer($"Getting aggregate weights for {aggType} in {season}");

                return Ok(standings);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }
        }

        // POST api/values
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody] List<MatchResultInputDto> results)
        {
            StartTimer();

            var errors = new List<string>();

            try
            {
                var matchResults = _mapper.Map<List<MatchResult>>(results);

                foreach (var result in matchResults)
                {
                    try
                    {
                        await _matchResultRepository.AddOrUpdateMatchResult(result);
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"{result.MatchId}, Member: {result.MembershipNumber} - {ex.Message}");
                    }
                }

            }
            catch (System.Exception ex)
            {
                errors.Add(ex.Message);

            }
            finally
            {
                ReportTimer("Posting match results");

            }

            if (errors.Any())
            {
                return BadRequest(errors);
            }
            else
            {
                return Ok();
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
