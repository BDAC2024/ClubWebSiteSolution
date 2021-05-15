using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : AnglingClubControllerBase
    {
        #region Backing Fields

        private readonly IHealthService _healthService;
        private readonly ILogger<HealthCheckController> _logger;

        #endregion

        #region Constructors

        public HealthCheckController(IHealthService healthService, ILoggerFactory loggerFactory)
        {
            _healthService = healthService;
            _logger = loggerFactory.CreateLogger<HealthCheckController>();
            base.Logger = _logger;
        }

        #endregion

        #region Methods

        [AllowAnonymous]
        [HttpGet]
        public void Get()
        {
            StartTimer();

            _healthService.CheckHealth();

            ReportTimer("Health Check");

            return;
        }



        #endregion
    }
}
