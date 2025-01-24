using AnglingClubShared.Enums;
using AnglingClubWebServices.DTOs;
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

            try
            {
                var match = _eventRepository.GetEvents().Result.Single(x => x.Id == matchId);
                var results = _mapper.Map<List<MatchResultOutputDto>>(_matchResultService.GetResults(matchId, match.MatchType.GetValueOrDefault(MatchType.Club)));
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
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody]List<MatchResultInputDto> results)
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
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
