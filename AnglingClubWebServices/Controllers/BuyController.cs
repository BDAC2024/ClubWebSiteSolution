using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
        private readonly IOrderRepository _orderRepository;
        private readonly ITmpFileRepository _savedFileRepository;
        private readonly IAuthService _authService;

        public BuyController(
            IOptions<StripeOptions> opts,
            IEmailService emailService,
            IAppSettingRepository appSettingRepository,
            ITmpFileRepository savedFileRepository,
            ILoggerFactory loggerFactory,
            ITicketService ticketService,
            IPaymentsService paymentsService,
            IProductMembershipRepository productMembershipRepository,
            IOrderRepository orderRepository,
            IAuthService authService)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _emailService = emailService;
            _appSettingRepository = appSettingRepository;
            _logger = loggerFactory.CreateLogger<BuyController>();
            base.Logger = _logger;
            _ticketService = ticketService;
            _paymentsService = paymentsService;
            _productMembershipRepository = productMembershipRepository;
            _orderRepository = orderRepository;
            _savedFileRepository = savedFileRepository;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("Membership")]
        public async Task<IActionResult> Membership([FromForm] NewMembershipDto membership, IFormFile disabilityCertificateFile)
        {
            StartTimer();

            var selectedMembership = _productMembershipRepository.GetProductMemberships().Result.FirstOrDefault(x => x.DbKey == membership.DbKey);

            if (selectedMembership == null) 
            {
                return BadRequest("Could not locate requested membership");
            }

            try
            {
                string disabilityCertificateFileAsString = "";
                string disabilityCertificateSavedFileId = "";

                try
                {
                    if (disabilityCertificateFile != null)
                    {
                        using var disabilityCertificateFileStream = disabilityCertificateFile.OpenReadStream();
                        using var ms = new MemoryStream();
                        disabilityCertificateFileStream.CopyTo(ms);
                        disabilityCertificateFileAsString = Convert.ToBase64String(ms.ToArray());
                        disabilityCertificateSavedFileId = Guid.NewGuid().ToString();
                        TmpFile disabilityCertificateSavedFile = new TmpFile { Id = disabilityCertificateSavedFileId, Content = disabilityCertificateFileAsString };

                        await _savedFileRepository.AddOrUpdateTmpFile(disabilityCertificateSavedFile);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Failed to save disabled certificate: {ex.InnerException.Message}");
                }

                try
                {
                    Dictionary<string, string> metaData = new Dictionary<string, string>()
                    {
                        { "Season", membership.SeasonName },
                        { "Name", membership.Name },
                        { "DoB", membership.DoB.ToString("yyyy-MM-dd") },
                        { "AcceptPolicies", membership.AcceptPolicies.ToString() },
                        { "PaidForKey", membership.PaidForKey.ToString() },
                    };

                    if (!disabilityCertificateFileAsString.IsNullOrEmpty())
                    {
                        metaData["DisabilityCertificateSavedFileId"] = disabilityCertificateSavedFileId;
                    }

                    if (membership.UnderAge)
                    {
                        metaData["ParentalConsent"] = membership.ParentalConsent.ToString();
                        metaData["UnderAge"] = membership.UnderAge.ToString();
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
                        metaData["AllowNameToBeUsed"] = membership.AllowNameToBeUsed.ToString();
                    }


                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCustomCheckoutSessionRequest
                    {
                        SuccessUrl = membership.SuccessUrl,
                        CancelUrl = membership.CancelUrl,
                        ProductPrice = selectedMembership.Cost,
                        ProductId = selectedMembership.Product,
                        AddCharges = true,
                        Mode = CheckoutType.Payment,
                        MetaData = metaData
                    }); ;

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

        [AllowAnonymous]
        [HttpPost("DayTicket")]
        [HttpPost]
        public async Task<IActionResult> DayTicket([FromBody] DayTicketDto ticket)
        {
            StartTimer();

            ticket.ValidOn = ticket.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!
            ticket.CallerBaseUrl = base.Caller;

            var appSettings = await _appSettingRepository.GetAppSettings();

            try
            {
                try
                {
                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCustomCheckoutSessionRequest
                    {
                        SuccessUrl = ticket.SuccessUrl,
                        CancelUrl = ticket.CancelUrl,
                        ProductPrice = appSettings.DayTicketCost,
                        ProductId = appSettings.ProductDayTicket,
                        Mode = CheckoutType.Payment,
                        MetaData = new Dictionary<string, string> {
                            { "HoldersName", ticket.HoldersName },
                            { "ValidOn", ticket.ValidOn.ToString("yyyy-MM-dd") },
                            { "Caller", ticket.CallerBaseUrl },
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

        [HttpPost]
        [HttpPost("GuestTicket")]
        public async Task<IActionResult> GuestTicket([FromBody] GuestTicketDto ticket)
        {
            StartTimer();

            ticket.ValidOn = ticket.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!

            var appSettings = await _appSettingRepository.GetAppSettings();

            try
            {
                try
                {
                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCustomCheckoutSessionRequest
                    {
                        SuccessUrl = ticket.SuccessUrl,
                        CancelUrl = ticket.CancelUrl,
                        ProductPrice = appSettings.GuestTicketCost,
                        ProductId = appSettings.ProductGuestTicket,
                        Mode = CheckoutType.Payment,
                        MetaData = new Dictionary<string, string> {
                            { "MembersName", ticket.MembersName },
                            { "MembershipNumber", CurrentUser.MembershipNumber.ToString() },
                            { "GuestsName", ticket.GuestsName },
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

        [AllowAnonymous]
        [HttpPost("PondGateKey")]
        [HttpPost]
        public async Task<IActionResult> PondGateKey([FromBody] PondGateKeyDto gateKey)
        {
            StartTimer();

            var appSettings = await _appSettingRepository.GetAppSettings();

            try
            {
                try
                {
                    var metaData = new Dictionary<string, string> {
                        { "Name", gateKey.Name },
                        { "AcceptPolicies", gateKey.AcceptPolicies.ToString()},
                        { "PotentialMember", gateKey.PotentialMember.ToString()},
                        { "PhoneNumber", gateKey.PhoneNumber }
                    };

                    if (CurrentUser != null)
                    {
                        metaData.Add("MembershipNumber", CurrentUser.MembershipNumber.ToString());
                    }

                    var sessionId = await _paymentsService.CreateCheckoutSession(new CreateCustomCheckoutSessionRequest 
                    {
                        SuccessUrl = gateKey.SuccessUrl,
                        CancelUrl = gateKey.CancelUrl,
                        Mode = CheckoutType.Payment,
                        MetaData = metaData,
                        ProductPrice = appSettings.PondGateKeyCost,
                        ProductId = appSettings.ProductPondGateKey

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
                ReportTimer("Buying pond gate key");
            }

        }

        [AllowAnonymous]
        [HttpPost("SendTicket")]
        [HttpPost]
        public IActionResult SendTicket([FromBody] OrderDto orderDto)
        {
            StartTimer();
            _logger.LogWarning("WebHookTimer - Starting to send ticket");

            try
            {
                var order = _orderRepository.GetOrder(orderDto.Id).Result;

                if (order.IssuedOn != null)
                {
                    return BadRequest($"This ticket was already issued on: {order.IssuedOn.Value.PrettyDate()}");
                }

                try
                {
                    if (order.PaymentId != orderDto.Key)
                    {
                        throw new Exception("SendTicket - unauthorised");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"SendTicket failed - key did not match. Was [{orderDto.Id}/{orderDto.Key}], expected [{order.DbKey}/{order.PaymentId}]");
                    _emailService.SendEmailToSupport("Failed to send a ticket - PLEASE INVESTIGATE ASAP", $"Key did not match expected value - see logs for detail. PLEASE INVESTIGATE ASAP");
                    throw;
                }

                var orderDetail = _paymentsService.GetDetail(order.DbKey).Result;

                sendOrderAsTicket(order, orderDetail, orderDto.CallerBaseUrl);
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
                ReportTimer("WebHookTimer - Sending ticket");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("HealthCheck")]
        [HttpPost]
        public IActionResult HealthCheck()
        {
            return Ok();
        }

        [HttpPost("ReSendTicket/{orderId}")]
        [HttpPost]
        public IActionResult ReSendTicket(int orderId)
        {
            StartTimer();
            _logger.LogWarning("WebHookTimer - Starting to re-send ticket");

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            Order order;

            try
            {
                order = _orderRepository.GetOrders().Result.FirstOrDefault(x => x.OrderId == orderId);

                if (order == null)
                {
                    var ex = new Exception("Cannot locate this ticket");
                    _logger.LogError(ex, $"Order id: {orderId} cannot be found so cannot re-send ticket");
                    throw ex;
                }

                var orderDetail = _paymentsService.GetDetail(order.DbKey).Result;

                sendOrderAsTicket(order, orderDetail, base.Caller);

                return Ok(_paymentsService.GetDetail(order.DbKey).Result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                ReportTimer("WebHookTimer - Re-Sending ticket");
            }

        }

        [HttpPost("EnableFeature/{featureType}/{enabled}")]
        [HttpPost]
        public IActionResult EnableFeature(PaymentType featureType, bool enabled)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            try
            {
                var appSettings = _appSettingRepository.GetAppSettings().Result;

                switch (featureType)
                {
                    case PaymentType.Membership:
                        appSettings.MembershipsEnabled = enabled;
                        break;

                    case PaymentType.GuestTicket:
                        appSettings.GuestTicketsEnabled = enabled;
                        break;

                    case PaymentType.DayTicket:
                        appSettings.DayTicketsEnabled = enabled;
                        break;

                    case PaymentType.PondGateKey:
                        appSettings.PondGateKeysEnabled = enabled;
                        break;

                    default:
                        break;
                }

                _appSettingRepository.AddOrUpdateAppSettings(appSettings).Wait();

                return Ok(enabled);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                ReportTimer("Enable/disable feature");
            }

        }

        [HttpGet("TestDayTicketClosures")]
        public IActionResult TestDayTicketClosures()
        {
            if (CurrentUser.Name != _authService.GetDeveloperName())
            {
                return Unauthorized();
            }

            _ticketService.IssueDayTicket(1, DateTime.Now, "holdersName", "emailAddress", "paymentId", "callerBaseUrl");

            return Ok();

        }

        private void sendOrderAsTicket(Order order, OrderDetailDto orderDetails, string callerBaseUrl)
        {
            order.IssuedOn = DateTime.Now; // Won't be committed unless send succeeds

            switch (order.OrderType)
            {
                case PaymentType.GuestTicket:
                    _logger.LogWarning($"Sending guest ticket...");
                    try
                    {
                        _ticketService.IssueGuestTicket(order.TicketNumber, order.ValidOn.Value, order.IssuedOn.Value, order.MembersName, order.GuestsName, orderDetails.MembershipNumber, orderDetails.Email, order.PaymentId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send a guest ticket for payment intent id: {order.PaymentId}");
                        _emailService.SendEmailToSupport("Failed to send a guest ticket - PLEASE INVESTIGATE ASAP", $"For ticket id: {order.TicketNumber}. Reason: {ex.Message}.<br/><br/>Ticket can be re-sent using the Payment Details view of this un-issued ticket.<br/><br/>PLEASE INVESTIGATE ASAP");
                        throw;
                    }
                    break;

                case PaymentType.DayTicket:
                    _logger.LogWarning($"Sending day ticket...");
                    try
                    {
                        _ticketService.IssueDayTicket(order.TicketNumber, order.ValidOn.Value, order.TicketHoldersName, orderDetails.Email, order.PaymentId, callerBaseUrl);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send a day ticket for payment intent id: {order.PaymentId}");
                        _emailService.SendEmailToSupport("Failed to send a day ticket - PLEASE INVESTIGATE ASAP", $"For ticket id: {order.TicketNumber}. Reason: {ex.Message}.<br/><br/>Ticket can be re-sent using the Payment Details view of this un-issued ticket.<br/><br/>PLEASE INVESTIGATE ASAP");
                        throw;
                    }
                    break;

                default:
                    break;
            }

            _orderRepository.AddOrUpdateOrder(order).Wait();

        }
    }
}
