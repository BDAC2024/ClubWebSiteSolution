using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models.Auth;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnglingClubWebServices.Services.UtilityService;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class MembersController : AnglingClubControllerBase
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUserAdminRepository _userAdminRepository;
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<MembersController> _logger;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IUtilityService _utilityService;

        public MembersController(
            IMemberRepository memberRepository,
            IUserAdminRepository userAdminRepository,
            IMatchResultRepository matchResultRepository,
            IEventRepository eventRepository,
            IMapper mapper,
            IAuthService authService,
            IEmailService emailService,
            ILoggerFactory loggerFactory,
            IReferenceDataRepository referenceDataRepository,
            IUtilityService utilityService)
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
            _referenceDataRepository = referenceDataRepository;
            _utilityService = utilityService;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            //try
            //{
            var response = await _authService.Authenticate(model);

            if (response == null)
            {
                return BadRequest(new { message = "Membership Number or PIN is incorrect" });
            }

            return Ok(response);
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

        [HttpGet("ExportRegisterCsv")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ExportRegisterCsv()
        {
            if (CurrentUser == null)
            {
                return Unauthorized();
            }

            var developerName = _authService.GetDeveloperName();
            if (string.IsNullOrWhiteSpace(developerName) || CurrentUser.Name != developerName)
            {
                throw new AppForbiddenException("Only the developer can export the member register.");
            }

            var members = (await _memberRepository.GetMembers())
                .OrderBy(member => member.MembershipNumber)
                .ThenBy(member => member.DbKey, StringComparer.Ordinal)
                .ToList();

            // Only export a per-file label when two or more records share a contact value.
            var contactGroups = members
                .Where(member => !string.IsNullOrWhiteSpace(member.Email))
                .GroupBy(member => member.Email.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Skip(1).Any())
                .Select((group, index) => new { group.Key, Label = $"Contact-{index + 1}" })
                .ToDictionary(group => group.Key, group => group.Label, StringComparer.OrdinalIgnoreCase);

            var csv = new StringBuilder("\uFEFFRecordKey,MembershipNumber,Name,SeasonValues,SharedContactGroup\r\n");
            foreach (var member in members)
            {
                var seasonValues = member.SeasonsActive
                    .Select(season => (int)season)
                    .Distinct()
                    .OrderBy(season => season)
                    .Select(season => season.ToString(CultureInfo.InvariantCulture));
                contactGroups.TryGetValue(member.Email.Trim(), out var contactGroup);

                csv.Append(EscapeRegisterCsvField(member.DbKey)).Append(',')
                    .Append(member.MembershipNumber.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(EscapeRegisterCsvField(member.Name)).Append(',')
                    .Append(EscapeRegisterCsvField(string.Join(", ", seasonValues))).Append(',')
                    .Append(contactGroup).Append("\r\n");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", "member-register.csv");
        }

        private static string EscapeRegisterCsvField(string value)
        {
            var trimmed = value.AsSpan().TrimStart();
            if (value.StartsWith('\t') || value.StartsWith('\r') || value.StartsWith('\n') ||
                (!trimmed.IsEmpty && trimmed[0] is '=' or '+' or '-' or '@'))
            {
                value = "'" + value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        [HttpGet("Merge/Candidates")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetMergeCandidates()
        {
            RequireMergeDeveloper();
            return Ok(MemberMergeCandidates.Find(await _memberRepository.GetMembers()));
        }

        [HttpGet("Merge/{membershipNumber:int}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetMergeRecords(int membershipNumber)
        {
            RequireMergeDeveloper();
            if (membershipNumber <= 0)
            {
                throw new AppValidationException("Enter a positive membership number.");
            }

            var members = (await _memberRepository.GetMembers()).Where(member => member.MembershipNumber == membershipNumber)
                .OrderBy(member => member.Name)
                .ThenBy(member => member.DbKey, StringComparer.Ordinal)
                .ToArray();
            var records = members.Select(member => new MemberMergeRecord(member.DbKey, member.Name,
                member.SeasonsActive.Distinct().OrderBy(season => season).ToArray())).ToArray();
            var pairs = MemberMergeCandidates.FindPairs(members)
                .Select(pair => new MemberMergePair(pair.First.DbKey, pair.Second.DbKey)).ToArray();
            return Ok(new MemberMergeRecords(records, pairs));
        }

        [HttpPost("Merge/Preview")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> PreviewMerge([FromBody] MemberMergeRequest request)
        {
            RequireMergeDeveloper();
            return Ok(await _memberRepository.PreviewMerge(request));
        }

        [HttpPost("Merge/Apply")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ApplyMerge([FromBody] MemberMergeRequest request)
        {
            RequireMergeDeveloper();
            await _memberRepository.ApplyMerge(request);
            return NoContent();
        }

        [HttpGet("Merge/Pending")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetPendingMerges()
        {
            RequireMergeDeveloper();
            return Ok(await _memberRepository.GetPendingMerges());
        }

        [HttpPost("Merge/Finalize")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> FinalizeMerge([FromBody] FinalizeMemberMergeRequest request)
        {
            RequireMergeDeveloper();
            await _memberRepository.FinalizeMerge(request);
            return NoContent();
        }

        private void RequireMergeDeveloper()
        {
            var developerName = _authService.GetDeveloperName();
            if (CurrentUser == null || string.IsNullOrWhiteSpace(developerName) || CurrentUser.Name != developerName)
            {
                throw new AppForbiddenException("Only the developer can merge member records.");
            }
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

            var allMembers = _memberRepository.GetMembers().Result;

            try
            {
                foreach (var member in members)
                {
                    var existingMember = allMembers.FirstOrDefault(x => x.MembershipNumber == member.MembershipNumber &&
                        x.Name.ToLower() == "anonymous" ? x.SeasonsActive.Contains(member.SeasonsActive.First()) : x.Name.ToLower() == member.Name.ToLower());

                    if (existingMember == null)
                    {
                        member.PinResetRequired = true;
                        _memberRepository.AddOrUpdateMember(member).Wait();
                    }
                    else
                    {
                        member.DbKey = existingMember.DbKey;

                        existingMember = allMembers.First(m => m.DbKey == member.DbKey);

                        // Update member when new value
                        if (member.SeasonsActive.Count == 1)
                        {
                            if (!existingMember.SeasonsActive.Contains(member.SeasonsActive.First()))
                            {
                                existingMember.SeasonsActive.Add(member.SeasonsActive.First());
                            }
                        }
                        else
                        {
                            member.SeasonsActive = member.SeasonsActive;
                        }

                        existingMember.Admin = member.Admin;
                        existingMember.MembershipNumber = member.MembershipNumber;

                        existingMember.Name = member.Name;

                        _memberRepository.AddOrUpdateMember(existingMember).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        public IActionResult Update([FromBody] MemberDto memberDto)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            try
            {
                var allMembers = _memberRepository.GetMembers().Result;

                var member = allMembers.First(m => m.DbKey == memberDto.DbKey);

                // Save these values to use when updating match results
                var orginalMembershipNumber = member.MembershipNumber;
                var originalSeasonsActive = member.SeasonsActive;

                // Update member when new value
                member.SeasonsActive = memberDto.SeasonsActive;

                member.Admin = memberDto.Admin;
                member.Treasurer = memberDto.Treasurer;
                member.CommitteeMember = memberDto.CommitteeMember;
                member.Secretary = memberDto.Secretary;
                member.MembershipSecretary = memberDto.MembershipSecretary;
                member.MatchSecretary = memberDto.MatchSecretary;
                member.Previewer = memberDto.Previewer;
                member.MembershipNumber = memberDto.MembershipNumber;
                member.ReLoginRequired = memberDto.ReLoginRequired;
                member.Email = memberDto.Email;

                member.Name = memberDto.Name;

                _memberRepository.AddOrUpdateMember(member).Wait();

                // Update all match results for this member in the members original seasons
                var resultsForMember = _matchResultRepository.GetAllMatchResults().Result
                                        .Where(x => x.MembershipNumber == orginalMembershipNumber);

                var allEvents = _eventRepository.GetEvents().Result;

                foreach (var result in resultsForMember)
                {
                    if (originalSeasonsActive.Contains(allEvents.First(x => x.Id == result.MatchId).Season))
                    {
                        if (result.MembershipNumber != member.MembershipNumber)
                        {
                            result.MembershipNumber = member.MembershipNumber;
                            _matchResultRepository.AddOrUpdateMatchResult(result).Wait();
                        }
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

                    _emailService.SendEmail(memberEmail, $"Your new PIN for Boroughbridge Angling Club", $"Your PIN has been reset to <b>{newPin}</b>. You will have to change this to a new PIN of your choice when you login.");
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

                _emailService.SendEmail(userAdmins, $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")}, PIN has been reset", $"User {member.MembershipNumber}{(member.AllowNameToBeUsed ? $" ({member.Name})" : "")} has a new PIN of <b>{newPin}</b>. Please contact them to inform them of their new PIN.<br/><br/><b>Note:</b> They will have to change this to a new PIN of their choice when they login.");

                ReportTimer("PIN reset done");

                return Ok(newPin);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot reset PIN");
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
                _logger.LogError(ex, "Cannot change PIN  of current user");
                return BadRequest("Sorry, PIN cannot be changed.");
            }
        }

        [HttpGet("PagesForBookPrinting/{numPages}/{printCoverSeparately}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PrintPaginationSummary>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult CalcPagesForBookPrinting(int numPages, bool printCoverSeparately)
        {
            StartTimer();

            if (!CurrentUser.MembershipSecretary)
            {
                return BadRequest("Only Member Secretary can access this.");
            }

            // Fixed at 4 for now
            var pagesPerSide = 4;

            List<PrintPaginationSummary> pageList = new List<PrintPaginationSummary>();
            try
            {
                pageList = _utilityService.CalcPagesForBookPrinting(numPages, pagesPerSide, printCoverSeparately);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            ReportTimer("Calc pages for printing");

            return Ok(pageList);


        }



    }
}
