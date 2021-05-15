using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class MembersController : AnglingClubControllerBase
    {
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MembersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;

        public MembersController(
            IMemberRepository memberRepository,
            IMapper mapper,
            IAuthService authService,
            ILoggerFactory loggerFactory)
        {
            _memberRepository = memberRepository;
            _mapper = mapper;
            _authService = authService;
            _logger = loggerFactory.CreateLogger<MembersController>();
            base.Logger = _logger;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody]AuthenticateRequest model)
        {
            var response = _authService.Authenticate(model).Result;

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Member>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var events = _memberRepository.GetMembers().Result;

            ReportTimer("Getting members");

            return Ok(events);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]List<Member> members)
        {
            StartTimer();

            foreach (var member in members)
            {
                _memberRepository.AddOrUpdateMember(member);
            }

            ReportTimer("Posting members");
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
