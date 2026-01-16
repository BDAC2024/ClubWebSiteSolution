using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class OpenMatchController : AnglingClubControllerBase
    {
        private readonly IOpenMatchRepository _openMatchRepository;
        private readonly IOpenMatchRegistrationRepository _openMatchRegistrationRepository;
        private readonly ILogger<OpenMatchController> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IUserAdminRepository _userAdminRepository;

        public OpenMatchController(
            IOpenMatchRepository openMatchRepository,
            IOpenMatchRegistrationRepository openMatchRegistrationRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IEmailService emailService,
            IUserAdminRepository userAdminRepository)
        {
            _openMatchRepository = openMatchRepository;
            _openMatchRegistrationRepository = openMatchRegistrationRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<OpenMatchController>();
            base.Logger = _logger;
            _emailService = emailService;
            _userAdminRepository = userAdminRepository;
        }

        [AllowAnonymous]
        [HttpGet("Matches/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OpenMatch>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(Season season)
        {
            StartTimer();

            var items = _openMatchRepository.GetOpenMatches().Result.Where(x => x.Season == season);

            foreach (var item in items)
            {
                item.PegsRemaining = pegsRemaining(item);
            }

            ReportTimer("Getting Open matches");

            return Ok(items);
        }

        [HttpGet("Registrations/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OpenMatchRegistration>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetRegistrations(Season season)
        {
            StartTimer();

            var matchIds = _openMatchRepository.GetOpenMatches().Result.Where(x => x.Season == season).Select(x => x.DbKey).ToList();
            var items = _openMatchRegistrationRepository.GetOpenMatchRegistrations().Result.Where(x => matchIds.Contains(x.OpenMatchId));

            ReportTimer("Getting Open matche registraions");

            return Ok(items.OrderBy(x => x.OpenMatchId));
        }

        [HttpPost("Matches")]
        public void Post([FromBody] List<OpenMatch> matches)
        {
            StartTimer();

            foreach (var match in matches)
            {
                _openMatchRepository.AddOrUpdateOpenMatch(match);
            }

            ReportTimer("Posting open matches");
        }

        [AllowAnonymous]
        [HttpPost("MatchRegistration")]
        public async Task<IActionResult> Post([FromBody] OpenMatchRegistration registration)
        {
            StartTimer();
            var openMatch = _openMatchRepository.GetOpenMatches().Result.Single(x => x.DbKey == registration.OpenMatchId);
            var currentRegistrations = _openMatchRegistrationRepository.GetOpenMatchRegistrations().Result.Where(x => x.OpenMatchId == registration.OpenMatchId);

            var pegsLeft = pegsRemaining(openMatch);

            if (pegsLeft < 1)
            {
                return BadRequest("This match is already fully booked");
            }
            else
            {
                try
                {
                    await _openMatchRegistrationRepository.AddOrUpdateOpenMatchRegistration(registration);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to add open match registration");
                    return BadRequest("Registration failed");
                }

                ReportTimer("Posting open match registration");

                // Email angler if email address given
                if (!string.IsNullOrEmpty(registration.ContactEmail))
                {
                    _emailService.SendEmail(new List<string> { registration.ContactEmail },
                        $"Confirmation of Junior Open Match registration for {openMatch.Date.ToString("dd MMM yyyy")}",
                        $"Thank you for registering for the match.<br/>" +
                        $"Remember that the draw is at <b>{openMatch.DrawTime}</b> and the match runs from <b>{openMatch.StartTime}</b> to <b>{openMatch.EndTime}</b><br/><br/>" +
                        $"Please familiarise yourself with the pond/match rules and the pond location at <a href='https://boroughbridgeanglingclub.com/register'>https://boroughbridgeanglingclub.com/register</a><br/><br/>" +
                        "Tight lines!,<br/>" +
                        "Boroughbridge & District Angling Club"
                    );
                }

                // Email user admins
                currentRegistrations = _openMatchRegistrationRepository.GetOpenMatchRegistrations().Result.Where(x => x.OpenMatchId == registration.OpenMatchId);
                var userAdmins = _userAdminRepository.GetUserAdmins().Result.Select(x => x.EmailAddress).ToList();
                var upTo12 = currentRegistrations.Count(x => x.AgeGroup == JuniorAgeGroup.UpTo12);
                var thirteenTo18 = currentRegistrations.Count(x => x.AgeGroup == JuniorAgeGroup.ThirteenTo18);
                var upTo12IsAre = upTo12 == 1 ? "is" : "are";
                var thirteenTo18IsAre = thirteenTo18 == 1 ? "is" : "are";

                _emailService.SendEmail(userAdmins,
                    $"New Junior Open Match registration for {openMatch.Date.ToString("dd MMM yyyy")} - {pegsRemaining(openMatch)} pegs left",
                    $"<b>{registration.Name}</b> has registered to fish the match on <b>{openMatch.Date.ToString("dd MMM yyyy")}</b><br/><br/>" +
                    $"There are <b>{pegsRemaining(openMatch)}</b> pegs remaining. Currently <b>{currentRegistrations.Count()}</b> registered; <b>{upTo12}</b> {upTo12IsAre} up to 12 and <b>{thirteenTo18}</b> {thirteenTo18IsAre} 13 to 18.<br/></br>" +
                    "Boroughbridge & District Angling Club"
                );

                return Ok(registration);
            }
        }
        [HttpDelete("MatchRegistration/{id}")]
        public void Delete(string id)
        {
            var errors = new List<string>();
            try
            {
                _openMatchRegistrationRepository.DeleteOpenMatchRegistration(id).Wait();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        private int pegsTaken(string openMatchId)
        {
            var allRegistrations = _openMatchRegistrationRepository.GetOpenMatchRegistrations().Result;
            var registrationsForThisMatch = allRegistrations.Where(x => x.OpenMatchId == openMatchId);
            return registrationsForThisMatch.Count();
        }

        private int pegsRemaining(OpenMatch match)
        {
            return match.PegsAvailable - pegsTaken(match.DbKey);
        }
    }
}
