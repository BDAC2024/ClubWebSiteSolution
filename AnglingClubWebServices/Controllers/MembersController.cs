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
        private readonly IUserAdminRepository _userAdminRepository;
        private readonly ILogger<MembersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public MembersController(
            IMemberRepository memberRepository,
            IUserAdminRepository userAdminRepository,
            IMapper mapper,
            IAuthService authService,
            IEmailService emailService,
            ILoggerFactory loggerFactory)
        {
            _memberRepository = memberRepository;
            _userAdminRepository = userAdminRepository;
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
            try
            {
                var response = _authService.Authenticate(model).Result;

                if (response == null)
                {
                    return BadRequest(new { message = "Membership Number or PIN is incorrect" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        // GET api/values
        [HttpGet("{onlyActive:bool?}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Member>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(bool onlyActive = false)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var members = _memberRepository.GetMembers().Result.Where(x => !onlyActive || x.Enabled).OrderBy(m => m.MembershipNumber).ToList();


            ReportTimer("Getting members");

            return Ok(members);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            StartTimer();

            if (!CurrentUser.Admin && id != CurrentUser.DbKey)
            {
                return BadRequest("You are not allowed to access this.");
            }

            var member = _memberRepository.GetMembers().Result.First(x => x.DbKey == id);

            ReportTimer("Getting member");

            return Ok(member);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]List<Member> members)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            foreach (var member in members)
            {
                _memberRepository.AddOrUpdateMember(member);
            }

            ReportTimer("Posting members");

            return Ok();
        }

        [HttpPost]
        [Route("Update")]
        public IActionResult Update([FromBody] Member memberDto)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var member = _memberRepository.GetMembers().Result.First(m => m.DbKey == memberDto.DbKey);

            member.LastPaid = memberDto.LastPaid;
            member.Admin = memberDto.Admin;
            member.Enabled = memberDto.Enabled;
            if (member.AllowNameToBeUsed)
            {
                member.Name = memberDto.Name;
            }

            _memberRepository.AddOrUpdateMember(member);

            ReportTimer("Updating member");

            return Ok();
        }

        [HttpPost]
        [Route("SetPreferences")]
        public IActionResult SetPreferences([FromBody] MemberPreferences prefs)
        {
            StartTimer();

            var member = (_memberRepository.GetMembers().Result).Single(x => x.DbKey == prefs.Id);

            if (member.DbKey != CurrentUser.DbKey)
            {
                return BadRequest("You cannot modify other member's details"); 
            }

            if (prefs.AllowNameToBeUsed == true && member.AllowNameToBeUsed == false)
            {
                // Notify admins of request to use member's name
                var memberEditUrl = $"http://localhost:4200/member/{member.MembershipNumber}";

                var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();

                _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}, action required", $"User {member.MembershipNumber} has requested their name be used in match results. <a href='{memberEditUrl}'>Please update them here</a>");
            }

            member.AllowNameToBeUsed = prefs.AllowNameToBeUsed;
            member.PreferencesLastUpdated = DateTime.Now;

            if (!member.AllowNameToBeUsed)
            {
                member.Name = "Anonymous";
            }

            _memberRepository.AddOrUpdateMember(member);

            ReportTimer("Updating member preferences");

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PinResetRequest/{membershipNumber}")]
        public IActionResult PinResetRequest(int membershipNumber)
        {
            StartTimer();

            try
            {
                var member = (_memberRepository.GetMembers().Result).Where(x => x.MembershipNumber == membershipNumber && x.Enabled).OrderByDescending(x => x.LastPaid).Take(1).SingleOrDefault();

                // Notify admins of request to use member's name
                var memberEditUrl = $"http://localhost:4200/member/{member.MembershipNumber}";

                var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();

                _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")}, PIN reset requested", $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")} has requested their PIN be reset. <a href='{memberEditUrl}'>Please reset their PIN here</a>");

                ReportTimer("PIN reset request");

                return Ok();

            }
            catch (Exception)
            {
                return BadRequest("Sorry, PIN reset cannot be requested. Did you enter the correct membership number?");
            }
        }

        [HttpPost]
        [Route("PinReset/{id}")]
        public IActionResult PinReset(string id)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            try
            {
                var member = (_memberRepository.GetMembers().Result).Single(x => x.DbKey == id);

                var newPin = member.NewPin();

                _memberRepository.AddOrUpdateMember(member);

                var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();

                _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")}, PIN has been reset", $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")} has a new PIN of <b>{newPin}</b>. Please contact them to inform them of their new PIN.<br/><br/><b>Note:</b> They will have have to change this to a new PIN of their choice when they login.");

                ReportTimer("PIN reset done");

                return Ok(newPin);

            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot reset PIN", ex);
                return BadRequest("Sorry, PIN reset cannot be done.");
            }
        }

        [HttpPost]
        [Route("SetNewPinOfCurrentUser/{newPin}")]
        public IActionResult SetNewPinOfCurrentUser(int newPin)
        {
            StartTimer();

            try
            {
                var member = (_memberRepository.GetMembers().Result).Single(x => x.DbKey == CurrentUser.DbKey);

                member.NewPin(newPin);

                _memberRepository.AddOrUpdateMember(member);

                ReportTimer("SetNewPinOfCurrentUser done");

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError("Cannot change PIN  of current user", ex);
                return BadRequest("Sorry, PIN cannot be changed.");
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
