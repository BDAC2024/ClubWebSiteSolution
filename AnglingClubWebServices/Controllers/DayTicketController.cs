using AnglingClubWebServices.Data;
using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class DayTicketController : AnglingClubControllerBase
    {
        private readonly ILogger<DayTicketController> _logger;
        private readonly IPaymentsService _paymentsService;
        private readonly IEmailService _emailService;
        private readonly ITicketService _ticketService;
        private readonly IAppSettingRepository _appSettingRepository;

        public DayTicketController(
            IOptions<StripeOptions> opts,
            IEmailService emailService,
            IAppSettingRepository appSettingRepository,
            ILoggerFactory loggerFactory,
            ITicketService ticketService,
            IPaymentsService paymentsService)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _emailService = emailService;
            _appSettingRepository = appSettingRepository;
            _logger = loggerFactory.CreateLogger<DayTicketController>();
            base.Logger = _logger;
            _ticketService = ticketService;
            _paymentsService = paymentsService;
        }

        //// Test returning the image file of the ticket
        //[AllowAnonymous]
        //[HttpGet]
        //public IActionResult Get()
        //{
        //    StartTimer();

        //    _ticketService.IssueDayTicket(DateTime.Now, "Steve API Test", "steve@townendmail.co.uk", DateTime.Now.Ticks.ToString());

        //    ReportTimer("Getting Day ticket image");

        //    return Ok();

        //}



    }
}
