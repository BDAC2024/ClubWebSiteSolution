using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class MembersController : AnglingClubControllerBase
    {
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MembersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public MembersController(
            IMemberRepository memberRepository,
            IMapper mapper,
            IAuthService authService,
            IEmailService emailService,
            ILoggerFactory loggerFactory)
        {
            _memberRepository = memberRepository;
            _mapper = mapper;
            _authService = authService;
            _emailService = emailService;
            _logger = loggerFactory.CreateLogger<MembersController>();
            base.Logger = _logger;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody]AuthenticateRequest model)
        {
            var response = _authService.Authenticate(model).Result;

            if (response == null)
                return BadRequest(new { message = "Membership Number or PIN is incorrect" });

            return Ok(response);
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Member>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var members = _memberRepository.GetMembers().Result;

            ReportTimer("Getting members");

            return Ok(members);
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

        [HttpPost]
        [Route("SetPreferences")]
        public void SetPreferences([FromBody] MemberPreferences prefs)
        {
            StartTimer();

            var member = (_memberRepository.GetMembers().Result).Single(x => x.DbKey == prefs.Id);


            if (prefs.AllowNameToBeUsed == true && member.AllowNameToBeUsed == false)
            {
                // Notify admins of request to use member's name
                var memberEditUrl = $"http://localhost:4200/member/{member.MembershipNumber}";

                _emailService.SendEmail("steve@townendmail.co.uk", $"User {member.MembershipNumber}, action required", $"User {member.MembershipNumber} has requested their name be used in match results. <a href='{memberEditUrl}'>Please update them here</a>");
            }

            member.AllowNameToBeUsed = prefs.AllowNameToBeUsed;
            member.PreferencesLastUpdated = DateTime.Now;

            if (!member.AllowNameToBeUsed)
            {
                member.Name = "Anonymous";
            }

            _memberRepository.AddOrUpdateMember(member);

            ReportTimer("Updating member preferences");
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
