using AnglingClubWebServices.Data;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class DayTicketController : AnglingClubControllerBase
    {
        private readonly ILogger<DayTicketController> _logger;
        private readonly IEmailService _emailService;
        private readonly ITicketService _ticketService;
        private readonly IAppSettingsRepository _appSettingsRepository;

        public DayTicketController(
            IEmailService emailService,
            IAppSettingsRepository appSettingsRepository,
            ILoggerFactory loggerFactory,
            ITicketService ticketService)
        {
            _emailService = emailService;
            _appSettingsRepository = appSettingsRepository;
            _logger = loggerFactory.CreateLogger<DayTicketController>();
            base.Logger = _logger;
            _ticketService = ticketService;
        }

        // Test returning the image file of the ticket
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            StartTimer();

            _ticketService.IssueDayTicket(DateTime.Now, "Steve API Test", "steve@townendmail.co.uk", "dummy6");

            ReportTimer("Getting Day ticket image");

            return Ok();

        }


    }
}
