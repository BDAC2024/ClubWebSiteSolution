using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class RulesController : AnglingClubControllerBase
    {
        private readonly IRulesRepository _rulesRepository;
        private readonly ILogger<RulesController> _logger;
        private readonly IMapper _mapper;

        public RulesController(
            IRulesRepository rulesRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _rulesRepository = rulesRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<RulesController>();
            base.Logger = _logger;
        }

        // GET api/values
        [AllowAnonymous]
        [HttpGet("{ruleType}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Rules>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(RuleType ruleType)
        {
            StartTimer();

            var items = _rulesRepository.GetRules().Result.Where(x => x.RuleType == ruleType);

            ReportTimer("Getting news items");

            return Ok(items);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]List<Rules> rulesItems)
        {
            StartTimer();

            foreach (var rulesItem in rulesItems)
            {
                _rulesRepository.AddOrUpdateRules(rulesItem);
            }

            ReportTimer("Posting rules");
        }

    }
}
