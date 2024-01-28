using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
                    var existingOrderForThisPayment = _orderRepository.GetOrders().Result.FirstOrDefault(x => x.PaymentId == paymentIntent.Id);

                    if (existingOrderForThisPayment != null)
                    {
                        order = existingOrderForThisPayment;
                    }
                    else
                    {
                        order = new Order();
                    }

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

                    _orderRepository.AddOrUpdateOrder(order).Wait();

                    var msg = $"A successful payment of £{order.Amount} for '{order.Description}' was made by {(order.OrderType == PaymentType.DayTicket ? order.TicketHoldersName : order.MembersName)} at {paymentIntent.ReceiptEmail}<br/><br/>";
                    _emailService.SendEmailToSupport("Payment received", msg);

                    
                    switch (paymentType)
                    {
                        case PaymentType.Membership:

                            break;

                        case PaymentType.GuestTicket:
                            _logger.LogWarning($"Sending guest ticket...");
                            try
                            {
                                _ticketService.IssueGuestTicket(order.ValidOn.Value, order.PaidOn.Value, order.MembersName, order.GuestsName, paymentMetaData.MembershipNumber, paymentIntent.ReceiptEmail, order.PaymentId);
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
                                _ticketService.IssueDayTicket(order.ValidOn.Value, order.TicketHoldersName, paymentIntent.ReceiptEmail, order.PaymentId);

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
                    
                }
                if (stripeEvent.Type == Events.ChargeRefunded)
                {
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
