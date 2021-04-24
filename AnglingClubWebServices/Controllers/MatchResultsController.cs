using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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
        public void Post([FromBody]List<MatchResultInputDto> results)
        {
            var matchResults = _mapper.Map<List<MatchResult>>(results);

            foreach (var result in matchResults)
            {
                _matchResultRepository.AddOrUpdateMatchResult(result);
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
