using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.DTOs;
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
using AnglingClubShared.Entities;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class EventsController : AnglingClubControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventsController> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        public EventsController(
            IEventRepository eventRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IEmailService emailService)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<EventsController>();
            base.Logger = _logger;
            _emailService = emailService;
        }

        // iCal export
        [HttpPost("export")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public IActionResult Export([FromBody]CalendarExportDto calendarExportDto)
        {
            StartTimer();

            List<EventType> selectedEventTypes = new List<EventType>();
            List<MatchType> selectedMatchTypes = new List<MatchType>();

            foreach (var exportType in calendarExportDto.selectedCalendarExportTypes)
            {
                _logger.LogInformation($"Type: {exportType.EnumDescription()}");

                switch (exportType)
                {
                    case CalendarExportType.All:
                        foreach (EventType val in Enum.GetValues(typeof(EventType)))
                        {
                            selectedEventTypes.Add(val);
                        }
                        break;

                    case CalendarExportType.AllMatches:
                        selectedEventTypes = new List<EventType>() { EventType.Work };
                        foreach (MatchType val in Enum.GetValues(typeof(MatchType)))
                        {
                            selectedMatchTypes.Add(val);
                        }
                        break;

                    case CalendarExportType.Meetings:
                        selectedEventTypes.AddRange(new List<EventType>() { EventType.Meeting, EventType.Work });
                        break;

                    case CalendarExportType.PondMatches:
                        selectedEventTypes.AddRange(new List<EventType>() { EventType.Work });
                        selectedMatchTypes.AddRange(new List<MatchType>() { MatchType.Spring, MatchType.Junior });
                        break;
                    case CalendarExportType.RiverMatches:
                        selectedEventTypes.AddRange(new List<EventType>() { EventType.Work });
                        selectedMatchTypes.AddRange(new List<MatchType>() { MatchType.Club, MatchType.OSU, MatchType.Specials, MatchType.Pairs, MatchType.Evening });
                        break;
                    default:
                        break;
                }
            }

            /* Test data
             * 
             * PROD
                Event:3a4e7925-9e1f-47d4-8f55-59cc1fd26fb2
                Event:2bce451b-8883-49b5-a65a-9d26647c3a65
                Event:fa764c8a-d7e6-4ae3-8fdc-529b5576907d
                Event:48c1a0f8-6a81-48b5-b88b-6f1ad7818be5

             * DEV
                Event:23aa99bb-4978-4f9d-96b6-c321df3d8733
                Event:76422e66-a73f-40da-a07d-91c8b11739c0
                Event:c9a28642-df73-4d66-a9e1-5b8ae295631e
                Event:0e7a20fa-d823-420f-aa7d-7f2a7f8bae18
             
             
             * Code
                var nonMatches = (_eventRepository.GetEvents().Result).Where(x => 
                    x.DbKey == "Event:23aa99bb-4978-4f9d-96b6-c321df3d8733" ||
                    x.DbKey == "Event:76422e66-a73f-40da-a07d-91c8b11739c0" ||
                    x.DbKey == "Event:c9a28642-df73-4d66-a9e1-5b8ae295631e" ||
                    x.DbKey == "Event:0e7a20fa-d823-420f-aa7d-7f2a7f8bae18");

                var matches = (_eventRepository.GetEvents().Result).Where(x => x.DbKey == "NoSuchItem");
             
             **/

            var nonMatches = (_eventRepository.GetEvents().Result).Where(x => x.Season == calendarExportDto.Season && selectedEventTypes.Contains(x.EventType));
            var matches = (_eventRepository.GetEvents().Result).Where(x => x.Season == calendarExportDto.Season && x.MatchType.HasValue && selectedMatchTypes.Contains(x.MatchType.Value));

            var calendar = new Calendar();

            _logger.LogInformation("Events:");
            foreach (var ev in nonMatches)
            {
                //_logger.LogInformation($"{ev.EventType.EnumDescription()} - {ev.Description} - {(ev.MatchType.HasValue ? ev.MatchType.EnumDescription() : "")}");

                var desc = ev.Description;
                if (ev.EventType == EventType.Match)
                {
                    desc = $"{ev.MatchType.EnumDescription()} no.{ev.Number}{(ev.Cup.IsNullOrEmpty() ? "" : " for ")}{ev.Cup}";
                }

                var icalEvent = new CalendarEvent
                {
                    Summary = $"BDAC {(ev.EventType == EventType.Match ? ev.MatchType.EnumDescription() : ev.EventType.EnumDescription())}: {ev.Description}",
                    Description = desc
                };

                if (ev.Date.ToShortTimeString() == "00:00")
                {
                    icalEvent.Start = new CalDateTime(DateOnly.FromDateTime(ev.Date));
                    icalEvent.End = new CalDateTime(DateOnly.FromDateTime(ev.Date.AddDays(1)));
                }
                else
                {
                    icalEvent.Start = new CalDateTime(ev.Date);
                }

                calendar.Events.Add(icalEvent);
            }

            _logger.LogInformation("Matches:");
            foreach (var ev in matches)
            {
                //_logger.LogInformation($"{ev.EventType.EnumDescription()} - {ev.Description} - {(ev.MatchType.HasValue ? ev.MatchType.EnumDescription() : "")}");

                var start = DateOnly.FromDateTime(ev.Date);
                var end = start.AddDays(1);

                var icalEvent = new CalendarEvent
                {
                    Summary = $"BDAC {ev.MatchType.EnumDescription()}: {ev.Description}",
                    Description = $"{ev.MatchType.EnumDescription()} no.{ev.Number}{(ev.Cup.IsNullOrEmpty() ? "" : " for ")}{ev.Cup}",
                    Start = new CalDateTime(start),
                    End = new CalDateTime(end)
                };

                calendar.Events.Add(icalEvent);
            }

            var iCalSerializer = new CalendarSerializer();
            string result = iCalSerializer.SerializeToString(calendar);

            _emailService.SendEmail(new List<string> { calendarExportDto.Email }, 
                "BDAC Calendar", 
                $"Hello,<br/><br/>" +
                    $"Please find attached the Boroughbridge & District Angling Club calendar for your selected events in the {EnumUtils.SeasonDisplay(calendarExportDto.Season)} season.<br/><br/>" +
                    "The email program on your phone/tablet/computer should recognise the attachment type and offer to load the events into your chosen calendar.<br/><br/>" +
                    "If your email program does not offer to load the events, many calendar programs have an import option that should be able to import the attached file.<br/><br/>" +
                    "Tight Lines,<br/>" +
                    "Boroughbridge & District Angling Club<br/>",
                null, null,
                new List<StreamAttachment> { new StreamAttachment { Filename = "BDAC_calendar.ics", ContentType = "text/calendar", Bytes = Encoding.ASCII.GetBytes(result) } });

            ReportTimer("Exporting events");

            return Ok();

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
