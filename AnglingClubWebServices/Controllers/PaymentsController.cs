using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class PaymentsController : AnglingClubControllerBase
    {
        private readonly IPaymentsService _paymentsService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IMapper _mapper;
        public PaymentsController(
            IPaymentsService paymentsService,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _paymentsService = paymentsService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<PaymentsController>();
            base.Logger = _logger;
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Payment>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            if (!CurrentUser.Admin)
            {
                return BadRequest("Only administrators can access this.");
            }

            var payments = _paymentsService.GetPayments();

            ReportTimer("Getting payments");

            return Ok(payments);
        }
    }
}
