using AnglingClubShared.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : AnglingClubControllerBase
    {
        #region Backing Fields

        private readonly RepositoryOptions _options;
        private readonly ILogger<AboutController> _logger;
        private readonly IAuthService _authService;

        #endregion

        #region Constructors

        public AboutController(IOptions<RepositoryOptions> opts, ILoggerFactory loggerFactory, IAuthService authService)
        {
            _options = opts.Value;
            _logger = loggerFactory.CreateLogger<AboutController>();
            _authService = authService;

            base.Logger = _logger;
        }

        #endregion

        #region Methods

        [HttpGet]
        public IActionResult Get()
        {
            StartTimer();

            var aboutInfo = new AboutDto();

            if (CurrentUser.Name == _authService.GetDeveloperName())
            {
                aboutInfo.Database = _options.SimpleDbDomain;
                aboutInfo.BackupBucket = _options.BackupBucket;
                aboutInfo.DocumentBucket = _options.DocumentBucket;
                aboutInfo.TmpFilesBucket = _options.TmpFilesBucket;
            }

            ReportTimer("About");

            return Ok(aboutInfo);
        }



        #endregion
    }
}
