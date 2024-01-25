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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DayTicketDto ticket)
        {
            StartTimer();

            ticket.ValidOn = ticket.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!

            var appSettings = await _appSettingRepository.GetAppSettings();

            try
            {
                try
                {
                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCheckoutSessionRequest 
                    {
                        SuccessUrl = ticket.SuccessUrl,
                        CancelUrl = ticket.CancelUrl,
                        PriceId = appSettings.ProductDayTicket,
                        Mode = CheckoutType.Payment,
                        MetaData = new Dictionary<string, string> {
                            { "HoldersName", ticket.HoldersName },
                            { "ValidOn", ticket.ValidOn.ToString("yyyy-MM-dd") },
                        }
                        
                    });

                    return Ok(new CreateCheckoutSessionResponse
                    {
                        SessionId = sessionId
                    });
                }
                catch (StripeException e)
                {
                    return BadRequest(e.StripeError.Message);
                }

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
                ReportTimer("Buying day ticket");
            }

        }


    }
}
