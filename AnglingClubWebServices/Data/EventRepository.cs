using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class EventRepository : RepositoryBase, IEventRepository
    {
        private const string IdPrefix = "Event";
        private readonly ILogger<EventRepository> _logger;

        private List<ClubEvent> _cachedEvents = null;

        public EventRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EventRepository>();
        }

        public async Task AddOrUpdateEvent(ClubEvent clubEvent)
        {
            var client = GetClient();

            try
            {

                // If this event already exists, grab its key
                {
                    _cachedEvents ??= await GetEvents();
                    var existingEvent = _cachedEvents.SingleOrDefault(x => x.Id == clubEvent.Id);
                    if (existingEvent != null)
                    {
                        clubEvent.DbKey = existingEvent.DbKey;
                    }
                }

                if (clubEvent.IsNewItem)
                {
                    clubEvent.DbKey = clubEvent.GenerateDbKey(IdPrefix);
                }

                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                // Mandatory properties
                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Id", Value = clubEvent.Id, Replace = true },
                    new ReplaceableAttribute { Name = "Season", Value = ((int)clubEvent.Season).ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "Date", Value = dateToString(clubEvent.Date), Replace = true },
                    new ReplaceableAttribute { Name = "EventType", Value = ((int)clubEvent.EventType).ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "Description", Value = clubEvent.Description, Replace = true }

                };

                // Optional properties
                if (clubEvent.MatchType != null) { attributes.Add(new ReplaceableAttribute { Name = "MatchType", Value = ((int)clubEvent.MatchType.Value).ToString(), Replace = true }); }
                if (clubEvent.AggregateType != null) { attributes.Add(new ReplaceableAttribute { Name = "AggregateWeightType", Value = ((int)clubEvent.AggregateType.Value).ToString(), Replace = true }); }
                if (clubEvent.MatchDraw != null) { attributes.Add(new ReplaceableAttribute { Name = "MatchDraw", Value = dateToString(clubEvent.MatchDraw.Value), Replace = true }); }
                if (clubEvent.MatchStart != null) { attributes.Add(new ReplaceableAttribute { Name = "MatchStart", Value = dateToString(clubEvent.MatchStart.Value), Replace = true }); }
                if (clubEvent.MatchEnd != null) { attributes.Add(new ReplaceableAttribute { Name = "MatchEnd", Value = dateToString(clubEvent.MatchEnd.Value), Replace = true }); }
                if (clubEvent.Number != null) { attributes.Add(new ReplaceableAttribute { Name = "Number", Value = numberToString(clubEvent.Number.Value), Replace = true }); }
                if (clubEvent.Cup != null) { attributes.Add(new ReplaceableAttribute { Name = "Cup", Value = clubEvent.Cup, Replace = true }); }

                request.Items.Add(
                    new ReplaceableItem
                    {
                        Name = clubEvent.DbKey,
                        Attributes = attributes
                    }
                ); 

                try
                {
                    await WriteInBatches(request, client);
                    //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                    _logger.LogDebug($"Event added: {clubEvent.DbKey} - {clubEvent.Description}");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to store event");
                throw;
            }

        }

        public async Task<List<ClubEvent>> GetEvents()
        {
            _logger.LogWarning($"Getting events at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var events = new List<ClubEvent>();

            var items = await GetData(IdPrefix, "AND Date > ''", "ORDER BY Date");

            foreach (var item in items)
            {
                var clubEvent = new ClubEvent();

                clubEvent.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Id":
                            clubEvent.Id = attribute.Value;
                            break;

                        case "Season":
                            clubEvent.Season = (Season)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Date":
                            clubEvent.Date = DateTime.Parse(attribute.Value);
                            break;

                        case "EventType":
                            clubEvent.EventType = (EventType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Description":
                            clubEvent.Description = attribute.Value;
                            break;

                        case "MatchType":
                            clubEvent.MatchType = (MatchType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "AggregateWeightType":
                            clubEvent.AggregateType = (AggregateType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "MatchDraw":
                            clubEvent.MatchDraw = DateTime.Parse(attribute.Value);
                            break;

                        case "MatchStart":
                            clubEvent.MatchStart = DateTime.Parse(attribute.Value);
                            break;

                        case "MatchEnd":
                            clubEvent.MatchEnd = DateTime.Parse(attribute.Value);
                            break;

                        case "Number":
                            clubEvent.Number = Convert.ToInt32(attribute.Value);
                            break;

                        case "Cup":
                            clubEvent.Cup = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                events.Add(clubEvent);
            }

            return events;

        }


    }
}
