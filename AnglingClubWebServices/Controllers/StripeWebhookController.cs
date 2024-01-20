using System;
using System.IO;
using System.Linq;
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
        public WebHookController(IOptions<StripeOptions> opts, IEmailService emailService, ILoggerFactory loggerFactory)
        {
            _emailService = emailService;
            
            _endpointSecret = opts.Value.StripeWebHookEndpointSecret;
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;

            _logger = loggerFactory.CreateLogger<WebHookController>();
            base.Logger = _logger;

            _logger.LogWarning($"Inside CTOR for WebHookController");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
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
                    var purchaseItem = lineItems.First().Description;

                    //_logger.LogWarning($"A successful payment for {paymentIntent.Amount} was made");
                    var msg = $"A successful payment for £{paymentIntent.Amount / 100.0} for '{purchaseItem}' was made by {paymentIntent.Shipping.Name} at {paymentIntent.ReceiptEmail}";
                    _logger.LogWarning(msg);
                    //Console.WriteLine("A successful payment for {0} was made.", paymentIntent.Amount);
                    _emailService.SendEmailToSupport("Payment received", msg);
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
                return BadRequest();
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
