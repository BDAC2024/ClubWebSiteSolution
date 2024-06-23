using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
using Org.BouncyCastle.Asn1.Ocsp;
using Stripe;
using Stripe.Checkout;
//using static AutoMapper.Internal.ExpressionFactory;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : AnglingClubControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly string _endpointSecret;
        private readonly ILogger<WebHookController> _logger;
        private readonly ITicketService _ticketService;
        private readonly IOrderRepository _orderRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IMemberRepository _memberRepository;

        public WebHookController(
            IOptions<StripeOptions> opts,
            IEmailService emailService,
            ILoggerFactory loggerFactory,
            ITicketService ticketService,
            IOrderRepository orderRepository,
            IAppSettingRepository appSettingRepository,
            IMemberRepository memberRepository)
        {
            _emailService = emailService;

            _endpointSecret = opts.Value.StripeWebHookEndpointSecret;
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _logger = loggerFactory.CreateLogger<WebHookController>();
            base.Logger = _logger;

            _logger.LogWarning($"Inside CTOR for WebHookController");
            _ticketService = ticketService;
            _logger.LogWarning($"Finished CTOR for WebHookController");
            _orderRepository = orderRepository;
            _appSettingRepository = appSettingRepository;
            _memberRepository = memberRepository;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var sw = new Stopwatch();

            _logger.LogWarning($"Inside Index for WebHookController - 1");

            _logger.LogWarning($"About to get json - log 2");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            _logger.LogWarning($"Got json - log 5");
            //_logger.LogWarning($"json - log 6 = {JsonSerializer.Serialize(json)}");
            //_logger.LogWarning($"Stripe-Signature - log 7 = {Request.Headers["Stripe-Signature"]}");
            //_logger.LogWarning($"_endpointSecret - log 8 = {_endpointSecret}");

            try
            {
                // Set this to false if webhooks have failed due to an api version mis-match. Once the outstanding items have been processed, set it back to true.
                bool throwOnApiVersionMismatch = true;

                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _endpointSecret, 300, throwOnApiVersionMismatch);

                _logger.LogWarning($"Got stripe event - log 10");

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    _logger.LogWarning($"Got paymentIntent - log 20");

                    var existingOrderForThisPayment = _orderRepository.GetOrders().Result.FirstOrDefault(x => x.PaymentId == paymentIntent.Id);

                    if (existingOrderForThisPayment != null)
                    {
                        _logger.LogWarning($"Ignoring webhook for {paymentIntent.Id} as it was already processed as order ID: {existingOrderForThisPayment.OrderId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Order for this payment doesnt exist so processing - log 30");

                        sw.Start();

                        var sessionService = new SessionService();
                        var sessionOptions = new Stripe.Checkout.SessionListOptions { Limit = 1, PaymentIntent = paymentIntent.Id};
                        sessionOptions.AddExpand("data.line_items");
                        sessionOptions.AddExpand("data.line_items.data.price");

                        StripeList<Session> sessions = sessionService.List(sessionOptions);

                        _logger.LogWarning($"Got sessions - log 40");

                        var productListOptions = new ProductListOptions { Ids = sessions.First().LineItems.Select(x => x.Price.ProductId).ToList() };
                        var productService = new ProductService();
                        var products = productService.List(productListOptions);
                        var product = products.First(x => x.Metadata.ContainsKey("Category"));

                        _logger.LogWarning($"Got product - log 50");

                        var category = product.Metadata.Where(m => m.Key == "Category").First().Value;
                        var paymentType = category.GetValueFromDescription<PaymentType>();

                        var purchaseItem = product.Name;

                        _logger.LogWarning($" log 60");

                        var chargeService = new ChargeService();
                        var charge = chargeService.Get(paymentIntent.LatestChargeId);

                        var paymentMetaData = new PaymentMetaData(paymentIntent.Metadata);

                        _logger.LogWarning($" log 70");

                        decimal fee = 0.0m;
                        try
                        {
                            _logger.LogWarning($" log 80");

                            var paymentIntentOptions = new PaymentIntentGetOptions();
                            paymentIntentOptions.AddExpand("latest_charge.balance_transaction");

                            var service = new PaymentIntentService();
                            PaymentIntent paymentIntentForFees = service.Get(paymentIntent.Id, paymentIntentOptions);
                            fee = paymentIntentForFees.LatestCharge.BalanceTransaction.FeeDetails.Sum(x => x.Amount) / 100.0m;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Unable to get the fee for payment Id {paymentIntent.Id} ", ex);
                            // continue to create order
                        }

                        _logger.LogWarning($" log 90");

                        var order = new Order();
                        order.OrderType = paymentType;

                        var existingOrders = _orderRepository.GetOrders().Result;
                        var latestOrderId = existingOrders.Any() ? existingOrders.Max(x => x.OrderId) : 0;

                        _logger.LogWarning($" log 100");

                        var existingOrdersOfThisType = existingOrders.Where(x => x.OrderType == order.OrderType);
                        var latestTicketNumber = existingOrdersOfThisType.Any() ? existingOrdersOfThisType.Max(x => x.TicketNumber) : 0;

                        _logger.LogWarning($" log 110");

                        order.OrderId = latestOrderId + 1;
                        order.Description = purchaseItem;
                        order.Amount = paymentIntent.Amount / 100.0m;
                        order.Fee = fee;
                        order.PaidOn = paymentIntent.Created;
                        order.PaymentId = paymentIntent.Id;
                        order.Status = charge.Paid ? "Paid" : "Failed";

                        _logger.LogWarning($" log 120 : order = {JsonSerializer.Serialize(order)}");

                        switch (paymentType)
                        {
                            case PaymentType.Membership:
                                {
                                    _logger.LogWarning($" log 130 - membership");

                                    order.MembersName = paymentMetaData.Name;

                                    var appSettings = _appSettingRepository.GetAppSettings().Result;
                                    var notificationSent = false;

                                    if (appSettings.MembershipSecretaries.Any())
                                    {
                                        var members = _memberRepository.GetMembers((Season?)EnumUtils.CurrentSeason()).Result.Where(x => appSettings.MembershipSecretaries.Contains(x.MembershipNumber));

                                        if (members.Any())
                                        {
                                            var emails = members.Select(x => x.Email).ToList();

                                            _emailService.SendEmail(
                                                emails,
                                                $"New membership has been purchased",
                                                $"A new <b>{order.Description}</b> has been purchased by/for <b>{order.MembersName}</b>.<br/>" +
                                                    "Full details can be found in the 'Payments' section of the Admin area of the website.<br/><br/>" +
                                                    "Boroughbridge & District Angling Club"
                                            );

                                            _emailService.SendEmail(
                                                new List<string> { paymentIntent.ReceiptEmail },
                                                $"Confirmation of membership purchase",
                                                $"Thank you for purchasing <b>{order.Description}</b> .<br/>" +
                                                    "Your membership book will soon be prepared and will be sent to you when ready.<br/><br/>" +
                                                    "Tight lines!,<br/>" +
                                                    "Boroughbridge & District Angling Club"
                                            );

                                            notificationSent = true;
                                        }
                                    }

                                    if (!notificationSent)
                                    {
                                        var exMsg = $"Failed to send an email to any member secretaries for payment intent: {paymentIntent.Id}";
                                        var ex = new Exception(exMsg);
                                        _logger.LogError(ex, exMsg);
                                        _emailService.SendEmailToSupport("Failed to notify new membership payment", $"For payment id: {paymentIntent.Id}, for {order.MembersName}. PLEASE INVESTIGATE ASAP");
                                        throw ex;
                                    }
                                    break;
                                }

                            case PaymentType.PondGateKey:
                                {
                                    _logger.LogWarning($" log 140 - PondGateKey");

                                    order.MembersName = paymentMetaData.Name;

                                    var appSettings = _appSettingRepository.GetAppSettings().Result;
                                    var notificationSent = false;

                                    if (appSettings.MembershipSecretaries.Any())
                                    {
                                        var members = _memberRepository.GetMembers((Season?)EnumUtils.CurrentSeason()).Result.Where(x => appSettings.MembershipSecretaries.Contains(x.MembershipNumber));

                                        if (members.Any())
                                        {
                                            var emails = members.Select(x => x.Email).ToList();

                                            _emailService.SendEmail(
                                                emails,
                                                $"New Pond Gate Key deposit has been purchased",
                                                $"A new <b>Pond Gate Key deposit</b> has been purchased by/for <b>{order.MembersName}</b>.<br/>" +
                                                    "Full details can be found in the 'Payments' section of the Admin area of the website.<br/><br/>" +
                                                    "Boroughbridge & District Angling Club"
                                            );

                                            _emailService.SendEmail(
                                                new List<string> { paymentIntent.ReceiptEmail },
                                                $"Confirmation of pond gate key deposit purchase",
                                                $"Thank you for purchasing <b>Pond Gate Key deposit</b> .<br/>" +
                                                    "Your key will be sent to you when ready.<br/><br/>" +
                                                    "Tight lines!,<br/>" +
                                                    "Boroughbridge & District Angling Club"
                                            );

                                            notificationSent = true;
                                        }
                                    }

                                    if (!notificationSent)
                                    {
                                        var exMsg = $"Failed to send an email to any member secretaries for payment intent: {paymentIntent.Id}";
                                        var ex = new Exception(exMsg);
                                        _logger.LogError(ex, exMsg);
                                        _emailService.SendEmailToSupport("Failed to notify new pond gate key deposit payment", $"For payment id: {paymentIntent.Id}, for {order.MembersName}. PLEASE INVESTIGATE ASAP");
                                        throw ex;
                                    }
                                    break;
                                }

                            case PaymentType.GuestTicket:

                                _logger.LogWarning($" log 150 - GuestTicket");

                                order.TicketNumber = latestTicketNumber + 1;
                                order.MembersName = paymentMetaData.MembersName;
                                order.GuestsName = paymentMetaData.GuestsName;
                                order.ValidOn = paymentMetaData.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!
                                break;

                            case PaymentType.DayTicket:

                                _logger.LogWarning($" log 160 - DayTicket");

                                order.TicketNumber = latestTicketNumber + 1;
                                order.TicketHoldersName = paymentMetaData.TicketHoldersName;
                                order.ValidOn = paymentMetaData.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!
                                break;

                            default:
                                break;
                        }

                        _logger.LogWarning($"WebHookTimer - creating order took : {sw.ElapsedMilliseconds} ms");
                        sw.Restart();

                        //var msg = $"A successful payment of £{order.Amount} for '{order.Description}' was made by {(order.OrderType == PaymentType.DayTicket ? order.TicketHoldersName : order.MembersName)} at {paymentIntent.ReceiptEmail}<br/><br/>";
                        //_emailService.SendEmailToSupport("Payment received", msg);

                        _logger.LogWarning($"WebHookTimer - sending support email took : {sw.ElapsedMilliseconds} ms");
                        sw.Restart();

                        // This will do a health check on the API and throw an exception if it fails. Therefore
                        // order won't be created and the webhook will fail. It should then try again automatically later.
                        var sharedClient = await getHttpClient();

                        _orderRepository.AddOrUpdateOrder(order).Wait();

                        _logger.LogWarning($"WebHookTimer - saving order took : {sw.ElapsedMilliseconds} ms");
                        sw.Restart();

                        switch (paymentType)
                        {
                            case PaymentType.Membership:

                                break;

                            case PaymentType.GuestTicket:
                                _logger.LogWarning($"Sending guest ticket...");
                                try
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                    // Note: This api will set order.IssuedOn if it succeeds
                                    sharedClient.PostAsJsonAsync(
                                        "buy/SendTicket",
                                        new OrderDto
                                        {
                                            Key = order.PaymentId,
                                            Id = order.DbKey,
                                            Email = paymentIntent.ReceiptEmail,
                                            MembershipNumber = paymentMetaData.MembershipNumber
                                        }
                                    );

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                    //_ticketService.IssueGuestTicket(order.TicketNumber, order.ValidOn.Value, order.IssuedOn.Value, order.MembersName, order.GuestsName, paymentMetaData.MembershipNumber, paymentIntent.ReceiptEmail, order.PaymentId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Failed to send a guest ticket for payment intent id: {paymentIntent.Id}");
                                    _emailService.SendEmailToSupport("Failed to send a guest ticket - PLEASE INVESTIGATE ASAP", $"For payment id: {paymentIntent.Id}. Reason: {ex.Message}. PLEASE INVESTIGATE ASAP");
                                    throw;
                                }
                                break;

                            case PaymentType.DayTicket:
                                _logger.LogWarning($"Sending day ticket...");
                                try
                                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                    // Note: This api will set order.IssuedOn if it succeeds
                                    sharedClient.PostAsJsonAsync(
                                        "buy/SendTicket",
                                        new OrderDto
                                        {
                                            Key = order.PaymentId,
                                            Id = order.DbKey,
                                            Email = paymentIntent.ReceiptEmail,
                                            CallerBaseUrl = paymentMetaData.Caller
                                        }
                                    );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                                    //_ticketService.IssueDayTicket(order.TicketNumber, order.ValidOn.Value, order.TicketHoldersName, paymentIntent.ReceiptEmail, order.PaymentId);

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Failed to send a day ticket for payment intent id: {paymentIntent.Id}");
                                    _emailService.SendEmailToSupport("Failed to send a day ticket - PLEASE INVESTIGATE ASAP", $"For payment id: {paymentIntent.Id}. Reason: {ex.Message}. PLEASE INVESTIGATE ASAP");
                                    throw;
                                }
                                break;

                            default:
                                break;
                        }

                        _logger.LogWarning($"WebHookTimer - sending ticket took : {sw.ElapsedMilliseconds} ms");
                        sw.Restart();

                    }

                }
                if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    _logger.LogWarning($"WebHookTimer - issuing a refund");

                    var charge = stripeEvent.Data.Object as Charge;

                    var existingOrder = _orderRepository.GetOrders().Result.FirstOrDefault(x => x.PaymentId == charge.PaymentIntentId);

                    if (existingOrder != null) 
                    {
                        existingOrder.Status = "Refunded";
                    }

                    _orderRepository.AddOrUpdateOrder(existingOrder).Wait();

                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook failed");
                return BadRequest(e.StripeError.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private async Task<HttpClient> getHttpClient()
        {
            var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
            var url = location.AbsoluteUri;
            _logger.LogWarning($"WebHookTimer - using url 2 : {url}");
            var baseUrl = url.ToLower().Replace("webhook", "");
            if (baseUrl.Contains("amazonaws"))
            {
                baseUrl = baseUrl.Replace("api/", "Prod/api/");
            }
            _logger.LogWarning($"WebHookTimer - using baseUrl 2 : {baseUrl}");

            HttpClient sharedClient = new()
            {
                BaseAddress = new Uri(baseUrl),
            };
            var healthResp = await sharedClient.PostAsJsonAsync("buy/HealthCheck", new { });

            if (!healthResp.IsSuccessStatusCode)
            {
                _logger.LogWarning($"WebHookTimer - ticket API call failed: {healthResp.StatusCode} - {healthResp.ReasonPhrase}");
                _logger.LogWarning($"WebHookTimer - resp uri: {healthResp.RequestMessage.RequestUri}");
                _logger.LogWarning($"WebHookTimer - resp method: {healthResp.RequestMessage.Method}");

                throw new Exception($"Ticket API health check failed: {healthResp.StatusCode} - {healthResp.ReasonPhrase}");
            }

            return sharedClient;
        }
    }


    /*
    [Route("api/[controller]")]
    public class WebHook : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                }
                else if (stripeEvent.Type == Events.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                }
                // ... handle other event types
                else
                {
                    // Unexpected event type
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
    */
}
