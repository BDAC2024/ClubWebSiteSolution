using AnglingClubWebServices.Controllers;
using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Services
{

    public class PaymentService : IPaymentsService
    {

        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOptions<StripeOptions> opts,
            ILoggerFactory loggerFactory)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;
            _logger = loggerFactory.CreateLogger<PaymentService>();
        }

        public List<Payment> GetPayments()
        {
            var payments = new List<Payment>();

            var sessionService = new SessionService();
            var sessionOptions = new Stripe.Checkout.SessionListOptions { Limit = 100};
            StripeList<Session> sessions = sessionService.List(sessionOptions);

            var completeSessions = sessions
                                    .Where(x => x.PaymentStatus == "paid")
                                    .Select(x => new { x.Id, x.PaymentIntentId, Name = x.CustomFields.Any() ? x.CustomFields.First().Text.Value : x.CustomerDetails.Name, x.CustomerDetails, x.AmountTotal, x.Created, x.Status, x.PaymentStatus }); ;

            var service = new ChargeService();
            var chargeOptions = new ChargeListOptions { Limit = 100 };
            StripeList<Charge> charges = service.List(chargeOptions); //.Where(x => x.Description != "(created by Stripe CLI)").ToList();

            var refundedCharges = charges.Where(x => x.Refunded == true).Select(x => new { x.PaymentIntentId });

            foreach (var session in completeSessions)
            {
                StripeList<LineItem> lineItems = sessionService.ListLineItems(session.Id);

                var purchase = lineItems.First().Description;

                var productService = new ProductService();
                var product = productService.Get(lineItems.First().Price.ProductId);
                var category = product.Metadata.Where(m => m.Key == "Category").First().Value;

                //var paymentIntentService = new PaymentIntentService();
                //var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                //var charge = service.Get(paymentIntent.LatestChargeId);

                var charge = charges.FirstOrDefault(c => c.PaymentIntentId == session.PaymentIntentId);

                var address = charge == null ? null : (charge.Shipping != null ? charge.Shipping.Address : charge.BillingDetails.Address);

                var paymentType = category.GetValueFromDescription<PaymentType>();
                var holdersName = "";
                var validOn = DateTime.MinValue;
                if (paymentType == PaymentType.DayTicket)
                {
                    if (charge.Metadata.Any(m => m.Key == "HoldersName"))
                    {
                        holdersName = charge.Metadata.Where(m => m.Key == "HoldersName").First().Value;
                    }
                    if (charge.Metadata.Any(m => m.Key == "ValidOn"))
                    {
                        DateTime.TryParse(charge.Metadata.Where(m => m.Key == "ValidOn").First().Value, out validOn);
                    }
                }

                payments.Add(new Payment
                {
                    SessionId = session.Id,
                    MembersName = session.Name,
                    HoldersName = holdersName,
                    ValidOn = validOn,
                    Email = session.CustomerDetails.Email,
                    Category = paymentType,
                    Purchase = purchase,
                    Amount = session.AmountTotal.Value / 100,
                    PaidOn = session.Created.ToLocalTime(),
                    Status = charge.Refunded ? "refunded" : $"{session.Status} ({session.PaymentStatus})",
                    CardHoldersName = charge == null ? "Unknown" : charge.BillingDetails.Name,
                    ShippingAddress = charge == null ? "Unknown" : $"{address.Line1}, {(address.Line2 != null ? address.Line2 + ", " : "")}{address.City}, {address.PostalCode}"
                });
            }

            return payments;
        }

        public async Task<string> CreateCheckoutSession(CreateCheckoutSessionRequest createCheckoutSessionRequest)
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = createCheckoutSessionRequest.SuccessUrl,
                CancelUrl = createCheckoutSessionRequest.CancelUrl,
                PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },
                Mode = createCheckoutSessionRequest.Mode.EnumDescription(),
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = createCheckoutSessionRequest.PriceId,
                            Quantity = 1
                        }
                    },
                BillingAddressCollection = "required",
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = createCheckoutSessionRequest.MetaData
                }
            };

            var service = new SessionService();

            try
            {
                var session = await service.CreateAsync(options);

                return session.Id;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Failed to setup a Day Ticket purchase session");
                throw;
            }

        }
    }
}
