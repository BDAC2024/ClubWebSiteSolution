using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebServices.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            StartTimer();

            List<MatchAllResultOutputDto> results = _matchResultService.GetResultsForAllMembers();

            ReportTimer("Getting all match results for all members");

            return Ok(results);
        }

        [HttpGet("memberResultsInSeason/{membershipNumber}/{aggType}/{season}/{basedOnPoints}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MemberResultsInSeason>))]
        public async Task<IActionResult> MemberResultsInSeason(int membershipNumber, AggregateType aggType, Season season, bool basedOnPoints)
        {
            StartTimer();

            var results = await _matchResultService.GetMemberResultsInSeason(membershipNumber, aggType, season, basedOnPoints);

            ReportTimer("Getting a members match results for a season");

            return Ok(results);
        }

        [HttpGet("standings/{aggType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeaguePosition>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetLeagueStandings(AggregateType aggType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            var standings = _matchResultService.GetLeagueStandings(aggType, season);

            ReportTimer($"Getting league standings for {aggType} in {season}");

            return Ok(standings);
        }

        [HttpGet("aggregateWeights/{aggType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AggregateWeight>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetAggregateWeights(AggregateType aggType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            var standings = _matchResultService.GetAggregateWeights(aggType, season);

            ReportTimer($"Getting aggregate weights for {aggType} in {season}");

            return Ok(standings);

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
