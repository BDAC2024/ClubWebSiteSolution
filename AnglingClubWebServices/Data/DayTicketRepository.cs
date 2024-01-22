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
    public class DayTicketRepository : RepositoryBase, IDayTicketRepository
    {
        private const string IdPrefix = "DayTicket";
        private readonly ILogger<DayTicketRepository> _logger;


        public DayTicketRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DayTicketRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get;  }

        public async Task AddOrUpdateTicket(DayTicket ticket)
        {
            var client = GetClient();

            if (ticket.IsNewItem)
            {
                ticket.DbKey = ticket.GenerateDbKey(IdPrefix);
            }


            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "TicketNumber", Value = ticket.TicketNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "PaymentId", Value = ticket.PaymentId, Replace = true },
                new ReplaceableAttribute { Name = "IssuedOn", Value = ticket.IssuedOn.HasValue ? dateToString(ticket.IssuedOn.Value) : "", Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = ticket.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"DayTicket added: {ticket.DbKey} - {ticket.TicketNumber}");

            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<DayTicket>> GetDayTickets(Season? season = null)
        {
            _logger.LogWarning($"Getting DayTickets at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var tickets = new List<DayTicket>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var dayTicket = new DayTicket();

                dayTicket.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "TicketNumber":
                            dayTicket.TicketNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "PaymentId":
                            dayTicket.PaymentId = attribute.Value;
                            break;

                        case "IssuedOn":
                            if (attribute.Value != "")
                            {
                                dayTicket.IssuedOn = DateTime.Parse(attribute.Value);
                            }
                            break;

                        default:
                            break;
                    }
                }

                tickets.Add(dayTicket);
            }

            if (season.HasValue)
            {
                return tickets.Where(x => x.Season == season).ToList();
            }
            else
            {
                return tickets.ToList();
            }
            

        }

        public async Task DeleteDayTicket(string id)
        {
            var client = GetClient();

            DeleteAttributesRequest request = new DeleteAttributesRequest();

            //request.Attributes.Add(new Amazon.SimpleDB.Model.Attribute { Name = id });
            request.DomainName = Domain;
            request.ItemName = id;

            try
            {
                DeleteAttributesResponse response = await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }


        }

    }
}
