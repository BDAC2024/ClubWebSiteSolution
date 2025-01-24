using AnglingClubShared.Extensions;
using AnglingClubWebServices.Controllers;
using AnglingClubWebServices.Data;
using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
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
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IAppSettingRepository _appSettingRepository;

        public PaymentService(
            IOptions<StripeOptions> opts,
            ILoggerFactory loggerFactory,
            IOrderRepository orderRepository,
            IMapper mapper,
            IAppSettingRepository appSettingRepository)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;
            _logger = loggerFactory.CreateLogger<PaymentService>();
            _orderRepository = orderRepository;
            _mapper = mapper;
            _appSettingRepository = appSettingRepository;
        }

        /// <summary>
        /// Returns the details of a payment by merging the minimal data held in the orders database entity with
        /// the related data from the stripe data for this payment.
        /// </summary>
        /// <param name="dbKey"></param>
        /// <returns></returns>
        public async Task<OrderDetailDto> GetDetail(string dbKey)
        {
            var detail = new OrderDetailDto();

            var order = (await _orderRepository.GetOrder(dbKey));

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Get(order.PaymentId);

            var chargeService = new ChargeService();
            var charge = chargeService.Get(paymentIntent.LatestChargeId);
            var paymentMetaData = new PaymentMetaData(charge.Metadata);

            _mapper.Map(paymentMetaData, detail);
            _mapper.Map(order, detail);

            detail.Description = order.Description;

            var address = charge == null ? null : (charge.Shipping != null ? charge.Shipping.Address : charge.BillingDetails.Address);
            detail.Address = address == null ? "Unknown" : $"{address.Line1}, {(address.Line2 != null ? address.Line2 + ", " : "")}{address.City}, {address.PostalCode}";

            detail.PaidOn = order.PaidOn;
            detail.Email = paymentIntent.ReceiptEmail;

            return detail;
        }


        /// <summary>
        /// Generates a checkout session in stripe for the requested product and returns the sessionId.
        /// The client (browser) can then redirect to that session to show the checkout page.
        /// </summary>
        /// <param name="createCheckoutSessionRequest"></param>
        /// <returns></returns>
        public async Task<string> CreateCheckoutSession(CreateCustomCheckoutSessionRequest createCheckoutSessionRequest)
        {
            var appSettings = await _appSettingRepository.GetAppSettings();

            var lineItems =
                new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "gbp",
                                UnitAmountDecimal = createCheckoutSessionRequest.ProductPrice * 100,
                                Product = createCheckoutSessionRequest.ProductId
                            },
                            Quantity = 1
                        }
                    };

            if (createCheckoutSessionRequest.MetaData.ContainsKey("PaidForKey") && createCheckoutSessionRequest.MetaData["PaidForKey"].Equals("True"))
            {
                lineItems.Add(
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "gbp",
                            UnitAmountDecimal = appSettings.PondGateKeyCost * 100,
                            Product = appSettings.ProductPondGateKey
                        },
                        Quantity = 1
                    });
            }

            if (createCheckoutSessionRequest.AddCharges)
            {
                lineItems.Add(
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "gbp",
                            UnitAmountDecimal = appSettings.HandlingCharge * 100,
                            Product = appSettings.ProductHandlingCharge
                        },
                        Quantity = 1
                    });
            }

            var options = new SessionCreateOptions
            {
                SuccessUrl = createCheckoutSessionRequest.SuccessUrl,
                CancelUrl = createCheckoutSessionRequest.CancelUrl,
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = createCheckoutSessionRequest.Mode.EnumDescription(),
                LineItems = lineItems,
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

        //public List<Payment> GetPayments()
        //{
        //    var payments = new List<Payment>();

        //    var sessionService = new SessionService();
        //    var sessionOptions = new Stripe.Checkout.SessionListOptions { Limit = 100};
        //    StripeList<Session> sessions = sessionService.List(sessionOptions);

        //    var completeSessions = sessions
        //                            .Where(x => x.PaymentStatus == "paid")
        //                            .Select(x => new { x.Id, x.PaymentIntentId, Name = x.CustomFields.Any() ? x.CustomFields.First().Text.Value : x.CustomerDetails.Name, x.CustomerDetails, x.AmountTotal, x.Created, x.Status, x.PaymentStatus }); ;

        //    var service = new ChargeService();
        //    var chargeOptions = new ChargeListOptions { Limit = 100 };
        //    StripeList<Charge> charges = service.List(chargeOptions); //.Where(x => x.Description != "(created by Stripe CLI)").ToList();

        //    var refundedCharges = charges.Where(x => x.Refunded == true).Select(x => new { x.PaymentIntentId });

        //    foreach (var session in completeSessions)
        //    {
        //        StripeList<LineItem> lineItems = sessionService.ListLineItems(session.Id);

        //        var purchase = lineItems.First().Description;

        //        var productService = new ProductService();
        //        var product = productService.Get(lineItems.First().Price.ProductId);
        //        var category = product.Metadata.Where(m => m.Key == "Category").First().Value;

        //        //var paymentIntentService = new PaymentIntentService();
        //        //var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
        //        //var charge = service.Get(paymentIntent.LatestChargeId);

        //        var charge = charges.FirstOrDefault(c => c.PaymentIntentId == session.PaymentIntentId);

        //        var address = charge == null ? null : (charge.Shipping != null ? charge.Shipping.Address : charge.BillingDetails.Address);

        //        var paymentType = category.GetValueFromDescription<PaymentType>();

        //        var paymentMetaData = new PaymentMetaData(charge.Metadata);

        //        payments.Add(new Payment
        //        {
        //            SessionId = session.Id,
        //            MembersName = string.IsNullOrEmpty(session.Name) ? paymentMetaData.MembersName : session.Name,
        //            MembershipNumber = paymentMetaData.MembershipNumber,
        //            HoldersName = paymentMetaData.TicketHoldersName,
        //            GuestsName = paymentMetaData.GuestsName,
        //            ValidOn = paymentMetaData.ValidOn,
        //            Email = session.CustomerDetails.Email,
        //            Category = paymentType,
        //            Purchase = purchase,
        //            Amount = session.AmountTotal.Value / 100,
        //            PaidOn = session.Created.ToLocalTime(),
        //            Status = charge.Refunded ? "refunded" : $"{session.Status} ({session.PaymentStatus})",
        //            CardHoldersName = charge == null ? "Unknown" : charge.BillingDetails.Name,
        //            ShippingAddress = charge == null ? "Unknown" : $"{address.Line1}, {(address.Line2 != null ? address.Line2 + ", " : "")}{address.City}, {address.PostalCode}"
        //        });
        //    }

        //    return payments;
        //}

        //public async Task<string> CreateCheckoutSession(CreateCheckoutSessionRequest createCheckoutSessionRequest)
        //{
        //    var options = new SessionCreateOptions
        //    {
        //        SuccessUrl = createCheckoutSessionRequest.SuccessUrl,
        //        CancelUrl = createCheckoutSessionRequest.CancelUrl,
        //        PaymentMethodTypes = new List<string>
        //            {
        //                "card"
        //            },
        //        Mode = createCheckoutSessionRequest.Mode.EnumDescription(),
        //        LineItems = new List<SessionLineItemOptions>
        //            {
        //                new SessionLineItemOptions
        //                {
        //                    Price = createCheckoutSessionRequest.PriceId,
        //                    Quantity = 1
        //                }
        //            },
        //        BillingAddressCollection = "required",
        //        PaymentIntentData = new SessionPaymentIntentDataOptions
        //        {
        //            Metadata = createCheckoutSessionRequest.MetaData
        //        }
        //    };

        //    var service = new SessionService();

        //    try
        //    {
        //        var session = await service.CreateAsync(options);

        //        return session.Id;
        //    }
        //    catch (StripeException e)
        //    {
        //        _logger.LogError(e, "Failed to setup a Day Ticket purchase session");
        //        throw;
        //    }

        //}
    }
}
