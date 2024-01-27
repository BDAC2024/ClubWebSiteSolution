﻿using AnglingClubWebServices.Controllers;
using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Helpers;
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

        public PaymentService(
            IOptions<StripeOptions> opts,
            ILoggerFactory loggerFactory,
            IOrderRepository orderRepository,
            IMapper mapper)
        {
            StripeConfiguration.ApiKey = opts.Value.StripeApiKey;
            _logger = loggerFactory.CreateLogger<PaymentService>();
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDetailDto> GetDetail(int orderId)
        {
            var detail = new OrderDetailDto();

            var order = (await _orderRepository.GetOrders()).FirstOrDefault(x => x.OrderId == orderId);

            if (order == null)
            {
                throw new Exception($"Cannot locate order: {orderId}");
            }

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Get(order.PaymentId);

            var chargeService = new ChargeService();
            var chargeOptions = new ChargeListOptions { Limit = 1 };
            var charge =chargeService.Get(paymentIntent.LatestChargeId);
            var paymentMetaData = new PaymentMetaData(charge.Metadata);

            _mapper.Map(paymentMetaData, detail);
            _mapper.Map(order, detail);

            detail.Description = order.Description;

            var address = charge == null ? null : (charge.Shipping != null ? charge.Shipping.Address : charge.BillingDetails.Address);
            detail.Address = address == null ? "Unknown" : $"{address.Line1}, {(address.Line2 != null ? address.Line2 + ", " : "")}{address.City}, {address.PostalCode}";

            detail.CreatedOn = order.CreatedOn;

            return detail;
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
