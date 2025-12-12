using AnglingClubShared.Enums;
using AnglingClubShared.DTOs;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class TrophyWinnersController : AnglingClubControllerBase
    {
        private readonly ILogger<TrophyWinnersController> _logger;
        private readonly IMapper _mapper;
        private readonly IMatchResultService _matchResultService;
        private readonly ITrophyWinnerRepository _trophyWinnerRepository;

        public TrophyWinnersController(
            ITrophyWinnerRepository trophyWinnerRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IMatchResultService matchResultService)
        {
            _trophyWinnerRepository = trophyWinnerRepository;
            _matchResultService = matchResultService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<TrophyWinnersController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet("{trophyType}/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TrophyWinner>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(TrophyType trophyType, Season season)
        {
            StartTimer();

            var trophyWinners = _matchResultService.GetTrophyWinners(trophyType, season);

            ReportTimer("Getting trophy winners");

            return Ok(trophyWinners);
        }

        // POST api/values
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody]List<TrophyWinner> trophyWinners)
        {
            StartTimer();
            var errors = new List<string>();
            var existingTrophyWinners = (_trophyWinnerRepository.GetTrophyWinners().Result);

            try
            {
                var winners = _mapper.Map<List<TrophyWinner>>(trophyWinners);

                foreach (var winner in winners)
                {
                    try
                    {
                        var existingWinner = existingTrophyWinners.FirstOrDefault(x => x.Season == winner.Season && x.Trophy == winner.Trophy);
                        if (existingWinner != null)
                        {
                            winner.DbKey = existingWinner.DbKey;
                        }

                        await _trophyWinnerRepository.AddOrUpdateTrophyWinner(winner);
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"{winner.Trophy} - {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                errors.Add(ex.Message);

            }
            finally
            {
                ReportTimer("Posting trophy winners");

            }
            if (errors.Any())
            {
                return BadRequest(errors);
            }
            else
            {
                return Ok();
            }
        }


    }
}
