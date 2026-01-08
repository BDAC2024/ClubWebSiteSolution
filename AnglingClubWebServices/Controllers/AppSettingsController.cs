using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class AppSettingsController : AnglingClubControllerBase
    {
        private readonly ILogger<AppSettingsController> _logger;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AppSettingsController(
            IAppSettingRepository appSettingRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IAuthService authService,
            IMemberRepository memberRepository)
        {
            _appSettingRepository = appSettingRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<AppSettingsController>();
            base.Logger = _logger;
            _authService = authService;
            _memberRepository = memberRepository;
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
        public async Task<IActionResult> PopulateClosureTimes()
        {
            if (CurrentUser.Name != _authService.GetDeveloperName())
            {
                return Unauthorized();
            }

            var currentSettings = await _appSettingRepository.GetAppSettings();
            if (currentSettings != null)
            {
                currentSettings.DayTicketClosureTimesPerMonth = "4pm,5pm,6pm,CLOSED,CLOSED,9pm,9pm,9pm,7pm,6pm,4pm,4pm";
                await _appSettingRepository.AddOrUpdateAppSettings(currentSettings);
            }

            return Ok();

        }

        /// <summary>
        /// Prepopulates the CommitteeMembers appSettings with standard values.
        /// </summary>
        /// <returns></returns>
        [HttpGet("PopulateCommitteeMembers")]
        public async Task<IActionResult> PopulateCommitteeMembers([FromBody] AppSettingListDto committeeMembers)
        {
            if (CurrentUser.Name != _authService.GetDeveloperName())
            {
                return Unauthorized();
            }

            var missingMembers = new List<string>();
            var foundMembers = new List<string>();

            var allMembers = await _memberRepository.GetMembers(EnumUtils.CurrentSeason());
            var committeeMemberIds = new List<int>();
            foreach (var name in committeeMembers.Names)
            {
                var member = allMembers.Find(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                {
                    committeeMemberIds.Add(member.MembershipNumber);
                    foundMembers.Add(name);
                }
                else 
                { 
                    missingMembers.Add(name);
                }
            }

            if (committeeMembers.AbortOnMissingNames && missingMembers.Any())
            {
                return BadRequest($"Aborting, only found: {String.Join(",", foundMembers.ToArray())}. These members not found: {String.Join(",", missingMembers.ToArray())}");
            }

            var currentSettings = await _appSettingRepository.GetAppSettings();
            if (currentSettings != null)
            {
                currentSettings.CommitteeMembers = committeeMemberIds;
                await _appSettingRepository.AddOrUpdateAppSettings(currentSettings);
            }

            if (missingMembers.Any())
            {
                return Ok($"For info, only added: {String.Join(",", foundMembers.ToArray())}. These members not found: {String.Join(",", missingMembers.ToArray())}");
            }
            else
            {
                return Ok();
            }

        }

        /// <summary>
        /// Prepopulates the Secretaries appSettings with standard values.
        /// </summary>
        /// <returns></returns>
        [HttpGet("PopulateSecretaries")]
        public async Task<IActionResult> PopulateSecretaries([FromBody] AppSettingListDto secretaries)
        {
            if (CurrentUser.Name != _authService.GetDeveloperName())
            {
                return Unauthorized();
            }

            var missingMembers = new List<string>();
            var foundMembers = new List<string>();

            var allMembers = await _memberRepository.GetMembers(EnumUtils.CurrentSeason());
            var secretaryMemberIds = new List<int>();
            foreach (var name in secretaries.Names)
            {
                var member = allMembers.Find(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                {
                    secretaryMemberIds.Add(member.MembershipNumber);
                    foundMembers.Add(name);
                }
                else
                {
                    missingMembers.Add(name);
                }
            }

            if (secretaries.AbortOnMissingNames && missingMembers.Any())
            {
                return BadRequest($"Aborting, only found: {String.Join(",", foundMembers.ToArray())}. These members not found: {String.Join(",", missingMembers.ToArray())}");
            }

            var currentSettings = await _appSettingRepository.GetAppSettings();
            if (currentSettings != null)
            {
                currentSettings.Secretaries = secretaryMemberIds;
                await _appSettingRepository.AddOrUpdateAppSettings(currentSettings);
            }

            if (missingMembers.Any())
            {
                return Ok($"For info, only added: {String.Join(",", foundMembers.ToArray())}. These members not found: {String.Join(",", missingMembers.ToArray())}");
            }
            else
            {
                return Ok();
            }

        }

    }
}
