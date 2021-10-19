using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

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
        [HttpGet("{season}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClubEvent>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Get(Season season)
        {
            StartTimer();

            var events = (_eventRepository.GetEvents().Result).Where(x => x.Season == season);

            ReportTimer("Getting events");

            return Ok(events);

            //var errors = new List<string>();
            //for (int i = 0; i < 5; i++)
            //{
            //    errors.Add($"Can't find {i}");
            //}

            //return BadRequest(errors);
        }

        // POST api/values
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync([FromBody]List<ClubEventInputDto> events)
        {
            StartTimer();
            var errors = new List<string>();

            try
            {
                var clubEvents = _mapper.Map<List<ClubEvent>>(events);

                foreach (var ev in clubEvents)
                {
                    try
                    {
                        await _eventRepository.AddOrUpdateEvent(ev);
                    }
                    catch (System.Exception ex)
                    {
                        errors.Add($"{ev.Id} - {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                errors.Add(ex.Message);

            }
            finally
            {
                ReportTimer("Posting events");

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
