using AnglingClubWebServices.DTOs;
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
    public class EventsController : AnglingClubControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventsController> _logger;
        private readonly IMapper _mapper;

        public EventsController(
            IEventRepository eventRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<EventsController>();
            base.Logger = _logger;
        }

        // GET api/values
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClubEvent>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get()
        {
            StartTimer();

            var events = _eventRepository.GetEvents().Result;

            ReportTimer("Getting events");

            return Ok(events);

            //var errors = new List<string>();
            //for (int i = 0; i < 5; i++)
            //{
            //    errors.Add($"Can't find {i}");
            //}

            //return BadRequest(errors);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]List<ClubEventInputDto> events)
        {
            StartTimer();

            var clubEvents = _mapper.Map<List<ClubEvent>>(events);

            foreach (var ev in clubEvents)
            {
                _eventRepository.AddOrUpdateEvent(ev);
            }

            ReportTimer("Posting events");
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
