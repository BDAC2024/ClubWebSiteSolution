using AnglingClubShared.Enums;
using AnglingClubShared.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class PaymentsController : AnglingClubControllerBase
    {
        private readonly IPaymentsService _paymentsService;
        private readonly IOrderRepository _orderRepository;

        private readonly ILogger<PaymentsController> _logger;
        private readonly IMapper _mapper;

        public PaymentsController(
            IPaymentsService paymentsService,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IOrderRepository orderRepository)
        {
            _paymentsService = paymentsService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<PaymentsController>();
            base.Logger = _logger;

            _orderRepository = orderRepository;
        }

        [HttpGet("GetForSeason/{season:int?}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Order>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetForSeason(Season? season)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var orders = _orderRepository.GetOrders(season).Result.ToList();

            if (season.HasValue)
            {
                orders = orders.Where(x => x.Season == season.Value).ToList();
            }

            ReportTimer("Getting payments");

            return Ok(orders.OrderByDescending(m => m.PaidOn).ToList());
        }

        [HttpGet("GetDetail/{dbKey}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDetailDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult GetDetail(string dbKey)
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            try
            {
                var detail = _paymentsService.GetDetail(dbKey).Result;

                ReportTimer("Getting payment detail");

                return Ok(detail);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
