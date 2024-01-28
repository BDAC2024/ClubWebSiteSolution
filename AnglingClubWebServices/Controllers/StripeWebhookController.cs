using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        public WebHookController(
            IOptions<StripeOptions> opts, 
            IEmailService emailService, 
            ILoggerFactory loggerFactory, 
            ITicketService ticketService, 
            IOrderRepository orderRepository
            )
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
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var sw = new Stopwatch();

            _logger.LogWarning($"Inside Index for WebHookController - 1");
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    var existingOrderForThisPayment = _orderRepository.GetOrders().Result.FirstOrDefault(x => x.PaymentId == paymentIntent.Id);

                    if (existingOrderForThisPayment != null)
                    {
                        _logger.LogWarning($"Ignoring webhook for {paymentIntent.Id} as it was already processed as order ID: {existingOrderForThisPayment.OrderId}");
                    }
                    else
                    {
                        sw.Start();

                        var sessionService = new SessionService();
                        var sessionOptions = new Stripe.Checkout.SessionListOptions { Limit = 1, PaymentIntent = paymentIntent.Id };
                        StripeList<Session> sessions = sessionService.List(sessionOptions);

                        StripeList<LineItem> lineItems = sessionService.ListLineItems(sessions.First().Id);

                        var productService = new ProductService();
                        var product = productService.Get(lineItems.First().Price.ProductId);
                        var category = product.Metadata.Where(m => m.Key == "Category").First().Value;
                        var paymentType = category.GetValueFromDescription<PaymentType>();

                        var purchaseItem = lineItems.First().Description;

                        var chargeService = new ChargeService();
                        var charge = chargeService.Get(paymentIntent.LatestChargeId);

                        var paymentMetaData = new PaymentMetaData(paymentIntent.Metadata);


                        var order = new Order();
                        order.OrderType = paymentType;

                        var existingOrders = _orderRepository.GetOrders().Result;
                        var latestOrderId = existingOrders.Any() ? existingOrders.Max(x => x.OrderId) : 0;

                        var existingOrdersOfThisType = existingOrders.Where(x => x.OrderType == order.OrderType);
                        var latestTicketNumber = existingOrdersOfThisType.Any() ? existingOrdersOfThisType.Max(x => x.TicketNumber) : 0;

                        order.OrderId = latestOrderId + 1;
                        order.Description = purchaseItem;
                        order.Amount = paymentIntent.Amount / 100.0m;
                        order.PaidOn = paymentIntent.Created;
                        order.PaymentId = paymentIntent.Id;
                        order.Status = charge.Paid ? "Paid" : "Failed";
                        switch (paymentType)
                        {
                            case PaymentType.Membership:
                                order.MembersName = paymentMetaData.Name;
                                break;

                            case PaymentType.GuestTicket:
                                order.TicketNumber = latestTicketNumber + 1;
                                order.MembersName = paymentMetaData.MembersName;
                                order.GuestsName = paymentMetaData.GuestsName;
                                order.ValidOn = paymentMetaData.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!
                                break;

                            case PaymentType.DayTicket:
                                order.TicketNumber = latestTicketNumber + 1;
                                order.TicketHoldersName = paymentMetaData.TicketHoldersName;
                                order.ValidOn = paymentMetaData.ValidOn.AddHours(12); // Ensure we don't get caught out by daylight savings!
                                break;

                            default:
                                break;
                        }

                        _logger.LogWarning($"WebHookTimer - creating order took : {sw.ElapsedMilliseconds} ms");
                        sw.Restart();

                        var msg = $"A successful payment of £{order.Amount} for '{order.Description}' was made by {(order.OrderType == PaymentType.DayTicket ? order.TicketHoldersName : order.MembersName)} at {paymentIntent.ReceiptEmail}<br/><br/>";
                        _emailService.SendEmailToSupport("Payment received", msg);

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
                                            Email = paymentIntent.ReceiptEmail
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
                _logger.LogError(e, $"Stripe webhook failed with: {e.StripeError.Message}");
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
