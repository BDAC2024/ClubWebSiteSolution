using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDAC.Common.Interfaces;
using BDAC.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly IWaterRepository _waterRepository;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(IWaterRepository waterRepository, ILoggerFactory loggerFactory)
        {
            _waterRepository = waterRepository;
            _logger = loggerFactory.CreateLogger<ValuesController>();
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _waterRepository.AddOrUpdateWater().Wait();

            _logger.LogInformation("All done!");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
