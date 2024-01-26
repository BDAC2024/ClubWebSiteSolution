using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{

    [Route("api/[controller]")]
    public class BuyController : AnglingClubControllerBase
    {
        private readonly ILogger<BuyController> _logger;
        private readonly IPaymentsService _paymentsService;
        private readonly IEmailService _emailService;
        private readonly ITicketService _ticketService;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IProductMembershipRepository _productMembershipRepository;

        public BuyController(
            IOptions<StripeOptions> opts,
            IEmailService emailService,
            IAppSettingRepository appSettingRepository,
            ILoggerFactory loggerFactory,
            ITicketService ticketService,
            IPaymentsService paymentsService,
            IProductMembershipRepository productMembershipRepository)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _emailService = emailService;
            _appSettingRepository = appSettingRepository;
            _logger = loggerFactory.CreateLogger<BuyController>();
            base.Logger = _logger;
            _ticketService = ticketService;
            _paymentsService = paymentsService;
            _productMembershipRepository = productMembershipRepository;
        }

        [AllowAnonymous]
        [HttpPost("Membership")]
        public async Task<IActionResult> Membership([FromBody] NewMembershipDto membership)
        {
            StartTimer();

            var selectedMembership = _productMembershipRepository.GetProductMemberships().Result.FirstOrDefault(x => x.DbKey == membership.DbKey);

            if (selectedMembership == null) 
            {
                return BadRequest("Could not locate requested membership");
            }

            try
            {
                try
                {
                    Dictionary<string, string> metaData = new Dictionary<string, string>()
                    {
                        { "Name", membership.Name },
                        { "DoB", membership.DoB.ToString("yyyy-MM-dd") },
                        { "AcceptPolicies", membership.AcceptPolicies.ToString() }
                    };

                    if (membership.UnderAge)
                    {
                        metaData["ParentalConsent"] = membership.ParentalConsent.ToString();
                        metaData["ChildCanSwim"] = membership.ChildCanSwim;
                        if (!string.IsNullOrEmpty(membership.Responsible1st)) { metaData["Responsible1st"] = membership.Responsible1st; }
                        if (!string.IsNullOrEmpty(membership.Responsible2nd)) { metaData["Responsible2nd"] = membership.Responsible2nd; }
                        if (!string.IsNullOrEmpty(membership.Responsible3rd)) { metaData["Responsible3rd"] = membership.Responsible3rd; }
                        if (!string.IsNullOrEmpty(membership.Responsible4th)) { metaData["Responsible4th"] = membership.Responsible4th; }
                        metaData["EmergencyContact"] = membership.EmergencyContact;
                        if (!string.IsNullOrEmpty(membership.EmergencyContactPhoneHome)) { metaData["EmergencyContactPhoneHome"] = membership.EmergencyContactPhoneHome; }
                        if (!string.IsNullOrEmpty(membership.EmergencyContactPhoneWork)) { metaData["EmergencyContactPhoneWork"] = membership.EmergencyContactPhoneWork; }
                        if (!string.IsNullOrEmpty(membership.EmergencyContactPhoneMobile)) { metaData["EmergencyContactPhoneMobile"] = membership.EmergencyContactPhoneMobile; }
                    }
                    else
                    {
                        metaData["PhoneNumber"] = membership.PhoneNumber;
                    }


                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCheckoutSessionRequest 
                    {
                        SuccessUrl = membership.SuccessUrl,
                        CancelUrl = membership.CancelUrl,
                        PriceId = selectedMembership.PriceId,
                        Mode = CheckoutType.Payment,
                        MetaData = metaData
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
                ReportTimer("Buying membership");
            }

        }


    }
}
