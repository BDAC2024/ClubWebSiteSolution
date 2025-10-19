using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class AppSettingsController : AnglingClubControllerBase
    {
        private readonly ILogger<AppSettingsController> _logger;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AppSettingsController(
            IAppSettingRepository appSettingRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IAuthService authService)
        {
            _appSettingRepository = appSettingRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<AppSettingsController>();
            base.Logger = _logger;
            _authService = authService;
        }

        // GET satisfied by RefData
        //[AllowAnonymous]
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NewsItem>))]
        //[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        //public IActionResult Get()
        //{
        //    StartTimer();

        //    var items = _newsRepository.GetNewsItems().Result;

        //    ReportTimer("Getting news items");

        //    return Ok(items);
        //}

        // GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/values
        [HttpPost]
        public void Post([FromBody]AppSettings appSettings)
        {
            StartTimer();

            _appSettingRepository.AddOrUpdateAppSettings(appSettings);

            ReportTimer("Posting app settings");
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var errors = new List<string>();

            try
            {
                _appSettingRepository.DeleteAppSetting(id).Wait();
            }
            catch (System.Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Prepopulates the ClosureTimes appSettings with standard values.
        /// </summary>
        /// <returns></returns>
        [HttpGet("PopulateClosureTimes")]
        public IActionResult PopulateClosureTimes()
        {
            if (CurrentUser.Name != _authService.GetDeveloperName())
            {
                return Unauthorized();
            }

            var currentSettings = _appSettingRepository.GetAppSettings().Result;
            if (currentSettings != null)
            {
                currentSettings.DayTicketClosureTimesPerMonth = "4pm,5pm,6pm,CLOSED,CLOSED,9pm,9pm,9pm,7pm,6pm,4pm,4pm";
                _appSettingRepository.AddOrUpdateAppSettings(currentSettings);
            }

            return Ok();

        }
    }
}
