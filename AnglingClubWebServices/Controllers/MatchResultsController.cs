using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class MatchResultsController : ControllerBase
    {
        private readonly ILogger<MatchResultsController> _logger;
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IMatchResultService _matchResultService;
        private readonly IMapper _mapper;

        public MatchResultsController(
            IMatchResultRepository matchResultRepository,
            IMatchResultService matchResultService,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _matchResultRepository = matchResultRepository;
            _matchResultService = matchResultService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<MatchResultsController>();
        }

        // GET api/values
        [HttpGet("{matchId}")]
        public IEnumerable<MatchResult> Get(string matchId)
        {
            var events = _matchResultService.GetResults(matchId);

            return events;
        }

        // POST api/values
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody]List<MatchResultInputDto> results)
        {
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
                        errors.Add(ex.Message);
                    }
                }

            }
            catch (System.Exception ex)
            {
                errors.Add(ex.Message);
                
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
