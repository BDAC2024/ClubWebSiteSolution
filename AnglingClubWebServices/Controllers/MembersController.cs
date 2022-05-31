using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Helpers;
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
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<MembersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public MembersController(
            IMemberRepository memberRepository,
            IUserAdminRepository userAdminRepository,
            IMatchResultRepository matchResultRepository,
            IEventRepository eventRepository,
            IMapper mapper,
            IAuthService authService,
            IEmailService emailService,
            ILoggerFactory loggerFactory)
        {
            _memberRepository = memberRepository;
            _userAdminRepository = userAdminRepository;
            _matchResultRepository = matchResultRepository;
            _eventRepository = eventRepository;
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

            Season? membersForSeason = onlyActive ? (Season?)EnumUtils.CurrentSeason() : null;

            var members = _memberRepository.GetMembers(membersForSeason).Result.OrderBy(m => m.MembershipNumber).ToList();

            ReportTimer("Getting members");

            return Ok(members);
        }

        // GET api/values
        [HttpGet("GetForSeason/{season:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Member>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetForSeason(Season season)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var members = _memberRepository.GetMembers(season).Result.OrderBy(m => m.MembershipNumber).ToList();

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

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post([FromBody] List<MemberDto> members)
        {
            StartTimer();

            bool accessAllowed = false;

            if ((members.Count() == 1 && members.First().Name == _authService.GetDeveloperName()) ||
                CurrentUser.Admin)
            {
                accessAllowed = true;
            }

            if (!accessAllowed)
            {
                return BadRequest("Only administrators can access this.");
            }

            foreach (var member in members)
            {
                member.PinResetRequired = true;
                _memberRepository.AddOrUpdateMember(member).Wait();
            }

            ReportTimer("Posting members");

            return Ok();
        }

        [HttpPost]
        [Route("UploadinitialPins")]
        public IActionResult UploadinitialPins([FromBody] List<MemberInitialPinDto> initialMemberPins)
        {
            StartTimer();

            bool accessAllowed = false;

            if (CurrentUser.Name == _authService.GetDeveloperName() && CurrentUser.Admin)
            {
                accessAllowed = true;
            }

            if (!accessAllowed)
            {
                return BadRequest("Only administrators can access this.");
            }

            foreach (var initialMemberPin in initialMemberPins)
            {
                var member = _memberRepository.GetMembers(EnumUtils.CurrentSeason()).Result.SingleOrDefault(x => x.MembershipNumber == initialMemberPin.MembershipNumber);

                if (member != null)
                {
                    // Check that the initial pin matches their current PIN and they do not appear to have logged in
                    if (member.ValidPin(initialMemberPin.InitialPin) 
                        && !member.AllowNameToBeUsed
                        && member.PinResetRequired
                        && string.IsNullOrEmpty(member.Email))
                    {
                        member.InitialPin = initialMemberPin.InitialPin;
                        _memberRepository.AddOrUpdateMember(member).Wait();
                        //_logger.LogWarning($"About to set initial PIN for member: {initialMemberPin.MembershipNumber} - {initialMemberPin.Name}");
                    }
                    else
                    {
                        _logger.LogWarning($"Cannot set initial PIN for member: {initialMemberPin.MembershipNumber} - {initialMemberPin.Name}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Cannot set initial PIN for member: {initialMemberPin.MembershipNumber} - {initialMemberPin.Name} - they cannot be found for current season");
                }
            }

            ReportTimer("Adding initial member pins");

            return Ok();
        }

        [HttpPost]
        [Route("AddMember")]
        public IActionResult AddMember([FromBody] MemberDto member)
        {
            StartTimer();

            bool accessAllowed = false;

            if (CurrentUser.Admin)
            {
                accessAllowed = true;
            }

            if (!accessAllowed)
            {
                return BadRequest("Only administrators can access this.");
            }

            member.PinResetRequired = true;
            member.InitialPin = member.NewPin();
            member.PreferencesLastUpdated = DateTime.MinValue;

            try
            {
                _memberRepository.AddOrUpdateMember(member).Wait();

                return Ok(member.InitialPin);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest(ex.InnerException.Message);
                }
                else
                {
                    return BadRequest(ex.Message);
                }
            }
            finally
            {
                ReportTimer("Posting member");
            }
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

            var allMembers = _memberRepository.GetMembers().Result;

            var member = allMembers.First(m => m.DbKey == memberDto.DbKey);

            // Save these values to use when updating match results
            var orginalMembershipNumber = member.MembershipNumber;
            var originalSeasonsActive = member.SeasonsActive;

            // Update member when new value
            member.SeasonsActive = memberDto.SeasonsActive;
            member.Admin = memberDto.Admin;
            member.MembershipNumber = memberDto.MembershipNumber;
            member.ReLoginRequired = memberDto.ReLoginRequired;
            member.Email = memberDto.Email;

            if (member.AllowNameToBeUsed)
            {
                member.Name = memberDto.Name;
            }

            try
            {
                _memberRepository.AddOrUpdateMember(member).Wait();

                // Update all match results for this member in the members original seasons
                var resultsForMember = _matchResultRepository.GetAllMatchResults().Result
                                        .Where(x => x.MembershipNumber == orginalMembershipNumber);


                foreach (var result in resultsForMember)
                {
                    if (originalSeasonsActive.Contains(_eventRepository.GetEvents().Result.First(x => x.Id == result.MatchId).Season))
                    {
                        result.MembershipNumber = member.MembershipNumber;
                        _matchResultRepository.AddOrUpdateMatchResult(result).Wait();
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            

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
                var memberEditUrl = $"{_memberRepository.SiteUrl}member/{member.DbKey}";

                var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();

                _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}, action required", $"User {member.MembershipNumber} has requested their name be used in match results. <a href='{memberEditUrl}'>Please update them here</a>");
            }

            member.AllowNameToBeUsed = prefs.AllowNameToBeUsed;
            member.Email = prefs.Email;
            member.PreferencesLastUpdated = DateTime.Now;

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
                var member = (_memberRepository.GetMembers(EnumUtils.CurrentSeason()).Result).Single(x => x.MembershipNumber == membershipNumber);
                var usingEMail = !string.IsNullOrEmpty(member.Email);

                if (usingEMail)
                {
                    var newPin = member.NewPin();
                    var memberEmail = new List<string> { member.Email };
                    _memberRepository.AddOrUpdateMember(member);

                    _emailService.SendEmail(memberEmail, $"Your new PIN for Boroughbridge Angling Club", $"Your PIN has been reset to <b>{newPin}</b>. You will have have to change this to a new PIN of your choice when you login.");
                }
                else
                {
                    var memberEditUrl = $"{_memberRepository.SiteUrl}member/{member.DbKey}";

                    var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();

                    _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")}, PIN reset requested", $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")} has requested their PIN be reset. <a href='{memberEditUrl}'>Please reset their PIN here</a>");

                    ReportTimer("PIN reset request");

                    member.PinResetRequested = true;

                    _memberRepository.AddOrUpdateMember(member).Wait();
                }

                return Ok(usingEMail);

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

                if (!member.PinResetRequested)
                {
                    return BadRequest("PIN reset has not been requested or has already been done.");
                }

                var newPin = member.NewPin();
                member.PinResetRequested = false;

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

            if (newPin < 1000)
            {
                return BadRequest("Sorry, PIN must be at least 4 digits and greater than 999");
            }

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
