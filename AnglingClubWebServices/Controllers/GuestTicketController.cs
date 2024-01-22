using AnglingClubWebServices.Data;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class GuestTicketController : AnglingClubControllerBase
    {
        private readonly IGuestTicketRepository _guestTicketRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<GuestTicketController> _logger;
        private readonly IEmailService _emailService;
        private readonly IAppSettingsRepository _appSettingsRepository;

        public GuestTicketController(
            IGuestTicketRepository guestTicketRepository,
            IMemberRepository memberRepository,
            IEmailService emailService,
            IAppSettingsRepository appSettingsRepository,
            ILoggerFactory loggerFactory)
        {
            _guestTicketRepository = guestTicketRepository;
            _memberRepository = memberRepository;
            _emailService = emailService;
            _appSettingsRepository = appSettingsRepository;
            _logger = loggerFactory.CreateLogger<GuestTicketController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet("{season:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GuestTicket>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(Season season)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var tickets = _guestTicketRepository.GetGuestTickets(season).Result.OrderBy(m => m.TicketValidOn).OrderByDescending(x => x.TicketNumber).ToList();

            ReportTimer("Getting guest tickets");

            return Ok(tickets);
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

            var ticket = _guestTicketRepository.GetGuestTickets().Result.First(x => x.DbKey == id);

            ReportTimer("Getting guest ticket");

            return Ok(ticket);
        }

        [HttpPost]
        public IActionResult Post([FromBody] GuestTicket ticket)
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

            try
            {
                ticket.IssuedOn = DateTime.Now;
                ticket.Cost = _appSettingsRepository.GetAppSettings().Result.GuestTicketCost;

                var issuer = _memberRepository.GetMembers(EnumUtils.SeasonForDate(ticket.IssuedOn).Value).Result.FirstOrDefault(x => x.MembershipNumber == CurrentUser.MembershipNumber);

                if (issuer == null)
                {
                    return BadRequest($"Issuer {ticket.IssuedByMembershipNumber} is not an active member for Season: {EnumUtils.SeasonDisplay(EnumUtils.SeasonForDate(ticket.IssuedOn).Value)}.");
                }
                ticket.IssuedBy = issuer.Name;

                var member = _memberRepository.GetMembers(ticket.Season).Result.FirstOrDefault(x => x.MembershipNumber == ticket.MembershipNumber);

                if (member == null)
                {
                    return BadRequest($"Member {ticket.MembershipNumber} is not an active member for Season: {EnumUtils.SeasonDisplay(ticket.Season)}.");
                }
                ticket.MembersName = member.Name;

                if (ticket.IsNewItem)
                {
                    var latestTicket = _guestTicketRepository.GetGuestTickets().Result.OrderByDescending(x => x.TicketNumber).FirstOrDefault();
                    ticket.TicketNumber = latestTicket != null ? latestTicket.TicketNumber + 1 : 1;
                }

                _guestTicketRepository.AddOrUpdateTicket(ticket).Wait();

                return Ok(_guestTicketRepository.GetGuestTickets().Result.First(x => x.DbKey == ticket.DbKey));
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
                ReportTimer("Posting guest ticket");
            }
        }

        [HttpPost]
        [Route("issue")]
        public IActionResult Issue([FromBody] GuestTicket ticket)
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

            try
            {
                _emailService.SendEmail(
                    new List<string> { ticket.EmailTo},
                    "Your Guest Ticket",
                    $"Please find attached, your guest ticket to take {ticket.GuestsName} fishing on {ticket.TicketValidOn:ddd MMM dd yyyy}.<br/>" +
                        "Make sure you have your ticket with you when fishing. Either on your phone or printed.<br/><br/>" +
                        "Tight lines!,<br/>" +
                        "Boroughbridge & District Angling Club",
                    null,
                    new List<ImageAttachment>
                    {
                        new ImageAttachment
                        {
                            Filename = $"Guest_Ticket_{ticket.TicketValidOn:yyyy_MM_dd}.png",
                            DataUrl = ticket.ImageData
                        }
                    }
                );

                return Ok();
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
                ReportTimer("Issuing guest ticket");
            }
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var errors = new List<string>();

            try
            {
                _guestTicketRepository.DeleteGuestTicket(id).Wait();
            }
            catch (System.Exception)
            {
                throw;
            }

        }

    }
}
