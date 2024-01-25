using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
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
        private readonly IMapper _mapper;

        public AppSettingsController(
            IAppSettingRepository appSettingRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _appSettingRepository = appSettingRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<AppSettingsController>();
            base.Logger = _logger;
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
    }
}
