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
                var results = _mapper.Map<List<MatchResultOutputDto>>(_matchResultService.GetResults(matchId));
                var match = _eventRepository.GetEvents().Result.Single(x => x.Id == matchId);
                var members = _memberRepository.GetMembers(match.Season).Result;
                foreach (var result in results)
                {
                    result.Name = members.Single(x => x.MembershipNumber == result.MembershipNumber).Name;
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

        [HttpGet("standings/{matchType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeaguePosition>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetLeagueStandings(MatchType matchType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            try
            {
                var standings = _matchResultService.GetLeagueStandings(matchType, season);

                ReportTimer("Getting league standings");

                return Ok(standings);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }
        }

        [HttpGet("aggregateWeights/{aggWeightType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AggregateWeight>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetAggregateWeights(AggregateWeightType aggWeightType, Season season)
        {
            var errors = new List<string>();

            StartTimer();

            try
            {
                var standings = _matchResultService.GetAggregateWeights(aggWeightType, season);

                ReportTimer("Getting aggregate weights");

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
