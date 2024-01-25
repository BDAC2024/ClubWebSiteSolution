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

        public WebHookController(IOptions<StripeOptions> opts, IEmailService emailService, ILoggerFactory loggerFactory, ITicketService ticketService)
        {
            _emailService = emailService;

            _endpointSecret = opts.Value.StripeWebHookEndpointSecret;
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _logger = loggerFactory.CreateLogger<WebHookController>();
            base.Logger = _logger;

            _logger.LogWarning($"Inside CTOR for WebHookController");
            _ticketService = ticketService;
            _logger.LogWarning($"Finished CTOR for WebHookController");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            _logger.LogWarning($"Inside Index for WebHookController - 1");
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                _logger.LogWarning($"Inside Index for WebHookController - 2");

                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    _logger.LogWarning($"Inside Index for WebHookController - 3");

                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    var sessionService = new SessionService();
                    var sessionOptions = new Stripe.Checkout.SessionListOptions { Limit = 1, PaymentIntent = paymentIntent.Id };
                    StripeList<Session> sessions = sessionService.List(sessionOptions);

                    _logger.LogWarning($"Inside Index for WebHookController - 4.1");

                    StripeList<LineItem> lineItems = sessionService.ListLineItems(sessions.First().Id);

                    var productService = new ProductService();
                    var product = productService.Get(lineItems.First().Price.ProductId);
                    var category = product.Metadata.Where(m => m.Key == "Category").First().Value;
                    var paymentType = category.GetValueFromDescription<PaymentType>();


                    _logger.LogWarning($"Inside Index for WebHookController - 3.5");


                    _logger.LogWarning($"Inside Index for WebHookController - 4");


                    var purchaseItem = lineItems.First().Description;

                    _logger.LogWarning($"Inside Index for WebHookController - 4.2");

                    var chargeService = new ChargeService();
                    var charge = chargeService.Get(paymentIntent.LatestChargeId);

                    _logger.LogWarning($"Inside Index for WebHookController - 4.25");

                    var name = paymentIntent.Shipping != null ? paymentIntent.Shipping.Name + " from Shipping" :
                        (paymentIntent.CustomerId != null ? paymentIntent.CustomerId + " from Customer" : 
                        (charge != null ? charge.BillingDetails.Name + " from Charge" : "Unknown" ));

                    //_logger.LogWarning($"A successful payment for {paymentIntent.Amount} was made");
                    //                    var msg = $"A successful payment for £{paymentIntent.Amount / 100.0} for '{purchaseItem}' was made by {paymentIntent.Shipping.Name} at {paymentIntent.ReceiptEmail}";
                    var msg = $"A successful payment for £{paymentIntent.Amount / 100.0} for '{purchaseItem}' was made by {name} at {paymentIntent.ReceiptEmail}<br/><br/>{JsonSerializer.Serialize(paymentIntent)}";

                    _logger.LogWarning($"Inside Index for WebHookController - 4.3");
                    _logger.LogWarning(msg);

                    _logger.LogWarning($"Inside Index for WebHookController - 4.4");

                    //Console.WriteLine("A successful payment for {0} was made.", paymentIntent.Amount);
                    _emailService.SendEmailToSupport("Payment received", msg);

                    _logger.LogWarning($"Inside Index for WebHookController - 5");

                    switch (paymentType)
                    {
                        case PaymentType.Membership:
                            break;
                        case PaymentType.GuestTicket:
                            _logger.LogWarning($"Sending guest ticket...");
                            try
                            {
                                DateTime validOn = DateTime.Parse(paymentIntent.Metadata["ValidOn"]);
                                int membershipNumber = Convert.ToInt32(paymentIntent.Metadata["MembershipNumber"]);

                                _ticketService.IssueGuestTicket(validOn, paymentIntent.Metadata["MembersName"], paymentIntent.Metadata["GuestsName"], membershipNumber, paymentIntent.ReceiptEmail, paymentIntent.Id);
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
                                DateTime validOn = DateTime.Parse(paymentIntent.Metadata["ValidOn"]);
                                _ticketService.IssueDayTicket(validOn, paymentIntent.Metadata["HoldersName"], paymentIntent.ReceiptEmail, paymentIntent.Id);

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to send a guest ticket for payment intent id: {paymentIntent.Id}");
                                _emailService.SendEmailToSupport("Failed to send a guest ticket - PLEASE INVESTIGATE ASAP", $"For payment id: {paymentIntent.Id}. Reason: {ex.Message}. PLEASE INVESTIGATE ASAP");
                                throw;
                            }
                            break;

                        default:
                            break;
                    }

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
