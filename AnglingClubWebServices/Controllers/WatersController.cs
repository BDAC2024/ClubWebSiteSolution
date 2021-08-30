using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatersController : AnglingClubControllerBase
    {
        private readonly IWaterRepository _waterRepository;
        private readonly ILogger<WatersController> _logger;

        public WatersController(
            IWaterRepository waterRepository, 
            ILoggerFactory loggerFactory)
        {
            _waterRepository = waterRepository;
            _logger = loggerFactory.CreateLogger<WatersController>();
            base.Logger = _logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WaterOutputDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            var waterDtos = new List<WaterOutputDto>();

            var errors = new List<string>();

            StartTimer();

            try
            {
                var waters = _waterRepository.GetWaters().Result;

                foreach (var water in waters)
                {
                    var dto = new WaterOutputDto();

                    dto.Access = water.Access;
                    dto.DbKey = water.DbKey;
                    dto.Description = water.Description;
                    dto.Directions = water.Directions;
                    dto.Id = water.Id;
                    dto.Name = water.Name;
                    dto.Species = water.Species;
                    dto.Type = water.Type;

                    var markers = water.Markers.Split('|');
                    var markerIcons = water.MarkerIcons.Split(',').ToList<string>();
                    var markerLabels = water.MarkerLabels.Split(',').ToList<string>();

                    for (int i = 0; i < markers.Length; i++)
                    {
                        var marker = markers[i].Split(',');
                        dto.Markers.Add(new Marker
                        {
                            Position = new Position { Lat = double.Parse(marker[0]), Long = double.Parse(marker[1]) },
                            Label = markerLabels[i],
                            Icon = markerIcons[i]
                        });
                    }


                    var dests = water.Destination.Split(',');
                    dto.Destination = new Position { Lat = double.Parse(dests[0]), Long = double.Parse(dests[1]) };
 
                    var paths = water.Path.Split('|');
                    for (int i = 0; i < paths.Length; i++)
                    {
                        var path = paths[i].Split(',');
                        dto.Path.Add(new Position { Lat = double.Parse(path[0]), Long = double.Parse(path[1]) });
                    }

                    waterDtos.Add(dto);
                }

                ReportTimer("Getting waters");

                return Ok(waterDtos);

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return BadRequest(errors);
            }

        }


        // GET api/<WatersController1>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<WatersController1>
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody] List<WaterInputDto> inputWaters)
        {
            StartTimer();

            List<Water> waters = new List<Water>();
            var errors = new List<string>();

            foreach (var inputWater in inputWaters)
            {
                var validInput = true;

                if ((inputWater.Markers.Count / 2) != inputWater.MarkerIcons.Count || (inputWater.Markers.Count / 2) != inputWater.MarkerLabels.Count)
                {
                    errors.Add($"{inputWater.Name} -  number of Markers, MarkerIcons and MarkerLabels should match but dont");
                    validInput = false;
                }

                if (inputWater.Destination.Count != 2)
                {
                    errors.Add($"{inputWater.Name} -  destination does not contain both latitude and longitude");
                    validInput = false;
                }

                if (inputWater.Path.Count % 2 != 0)
                {
                    errors.Add($"{inputWater.Name} -  path does not contain both latitude and longitude for all points");
                    validInput = false;
                }

                if (validInput)
                {
                    var water = new Water();

                    water.Id = inputWater.Id;
                    water.Name = inputWater.Name;
                    water.Type = inputWater.Type;
                    water.Access = inputWater.Access;
                    water.Description = inputWater.Description;
                    water.Species = inputWater.Species;
                    water.Directions = inputWater.Directions;

                    water.MarkerIcons = string.Join(",", inputWater.MarkerIcons.ToArray());
                    water.MarkerLabels = string.Join(",", inputWater.MarkerLabels.ToArray());
                    water.Destination = string.Join(",", inputWater.Destination.ToArray());

                    var markers = inputWater.Markers.ToArray();
                    var markerPositions = new List<string>();

                    for (int i = 0; i < inputWater.Markers.Count; i += 2)
                    {
                        markerPositions.Add($"{markers[i]},{markers[i + 1]}");
                    }
                    water.Markers = string.Join("|", markerPositions.ToArray());

                    var path = inputWater.Path.ToArray();
                    var positions = new List<string>();

                    for (int i = 0; i < inputWater.Path.Count; i+=2)
                    {
                        positions.Add($"{path[i]},{path[i+1]}");
                    }
                    water.Path = string.Join("|", positions.ToArray());

                    waters.Add(water);
                }

            }

            if (!errors.Any())
            {
                foreach (var water in waters)
                {
                    try
                    {
                        await _waterRepository.AddOrUpdateWater(water);

                        ReportTimer("Posting waters");
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
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

        // POST api/<WatersController1>
        [HttpPost("UpdateDescription")]
        public async System.Threading.Tasks.Task<IActionResult> UpdateWater([FromBody] WaterUpdateDto waterToUpdate)
        {
            StartTimer();

            List<Water> waters = new List<Water>();
            var errors = new List<string>();

            var water = _waterRepository.GetWaters().Result.Single(x => x.DbKey == waterToUpdate.DbKey);

            water.Description = waterToUpdate.Description;

            try
            {
                await _waterRepository.UpdateDesc(water);

                ReportTimer("Updating water desc");
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }


            water.Directions = waterToUpdate.Directions;
            try
            {
                await _waterRepository.UpdateDirections(water);

                ReportTimer("Updating water directions");
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
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


        // PUT api/<WatersController1>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<WatersController1>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
