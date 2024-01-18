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
    public class GuestTicketRepository : RepositoryBase, IGuestTicketRepository
    {
        private const string IdPrefix = "GuestTicket";
        private readonly ILogger<GuestTicketRepository> _logger;


        public GuestTicketRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GuestTicketRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get;  }

        public async Task AddOrUpdateTicket(GuestTicket guestTicket)
        {
            var client = GetClient();

            if (guestTicket.IsNewItem)
            {
                guestTicket.DbKey = guestTicket.GenerateDbKey(IdPrefix);
            }


            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "TicketNumber", Value = guestTicket.TicketNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Cost", Value = guestTicket.Cost.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "IssuedBy", Value = guestTicket.IssuedBy, Replace = true },
                new ReplaceableAttribute { Name = "IssuedByMembershipNumber", Value = guestTicket.IssuedByMembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "IssuedOn", Value = dateToString(guestTicket.IssuedOn), Replace = true },
                new ReplaceableAttribute { Name = "TicketValidOn", Value = dateToString(guestTicket.TicketValidOn), Replace = true },
                new ReplaceableAttribute { Name = "MembersName", Value = guestTicket.MembersName, Replace = true },
                new ReplaceableAttribute { Name = "MembershipNumber", Value = guestTicket.MembershipNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "EmailTo", Value = guestTicket.EmailTo, Replace = true },
                new ReplaceableAttribute { Name = "GuestsName", Value = guestTicket.GuestsName, Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = guestTicket.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"GuestTicket added: {guestTicket.DbKey} - {guestTicket.TicketNumber}");

            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<GuestTicket>> GetGuestTickets(Season? season = null)
        {
            _logger.LogWarning($"Getting GuestTickets at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var guestTickets = new List<GuestTicket>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var guestTicket = new GuestTicket();

                guestTicket.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "TicketNumber":
                            guestTicket.TicketNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "Cost":
                            guestTicket.Cost = Convert.ToDecimal(attribute.Value);
                            break;

                        case "IssuedBy":
                            guestTicket.IssuedBy = attribute.Value;
                            break;

                        case "IssuedByMembershipNumber":
                            guestTicket.IssuedByMembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "IssuedOn":
                            guestTicket.IssuedOn = DateTime.Parse(attribute.Value);
                            break;

                        case "TicketValidOn":
                            guestTicket.TicketValidOn = DateTime.Parse(attribute.Value);
                            break;

                        case "MembersName":
                            guestTicket.MembersName = attribute.Value;
                            break;

                        case "EmailTo":
                            guestTicket.EmailTo = attribute.Value;
                            break;

                        case "MembershipNumber":
                            guestTicket.MembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "GuestsName":
                            guestTicket.GuestsName = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                guestTickets.Add(guestTicket);
            }

            if (season.HasValue)
            {
                return guestTickets.Where(x => x.Season == season).ToList();
            }
            else
            {
                return guestTickets.ToList();
            }
            

        }

        public async Task DeleteGuestTicket(string id)
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
