using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
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

        // GET: api/<WatersController1>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
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

                if (inputWater.Icon.Count != inputWater.Label.Count)
                {
                    errors.Add($"{inputWater.Name} -  number of Icons and Labels should match but dont");
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

                    water.Icon = string.Join(",", inputWater.Icon.ToArray());
                    water.Label = string.Join(",", inputWater.Label.ToArray());
                    water.Destination = string.Join(",", inputWater.Destination.ToArray());

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
