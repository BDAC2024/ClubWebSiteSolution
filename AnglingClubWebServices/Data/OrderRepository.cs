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
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        private const string IdPrefix = "Order";
        private readonly ILogger<OrderRepository> _logger;


        public OrderRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OrderRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get;  }

        public async Task AddOrUpdateOrder(Order order)
        {
            var client = GetClient();

            if (order.IsNewItem)
            {
                order.DbKey = order.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "OrderId", Value = order.OrderId.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "OrderType", Value = ((int)order.OrderType).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Description", Value = order.Description, Replace = true },
                new ReplaceableAttribute { Name = "TicketNumber", Value = order.TicketNumber.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Season", Value = order.SeasonName, Replace = true },
                new ReplaceableAttribute { Name = "MembersName", Value = order.MembersName, Replace = true },
                new ReplaceableAttribute { Name = "GuestsName", Value = order.GuestsName, Replace = true },
                new ReplaceableAttribute { Name = "TicketHoldersName", Value = order.TicketHoldersName, Replace = true },
                new ReplaceableAttribute { Name = "Amount", Value = order.Amount.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Fee", Value = order.Fee.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "ValidOn", Value = order.ValidOn.HasValue ? dateToString(order.ValidOn.Value) : "", Replace = true },
                new ReplaceableAttribute { Name = "PaidOn", Value = order.PaidOn.HasValue ? dateToString(order.PaidOn.Value) : "", Replace = true },
                new ReplaceableAttribute { Name = "IssuedOn", Value = order.IssuedOn.HasValue ? dateToString(order.IssuedOn.Value) : "", Replace = true },
                new ReplaceableAttribute { Name = "PaymentId", Value = order.PaymentId, Replace = true },
                new ReplaceableAttribute { Name = "Status", Value = order.Status, Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = order.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"Order added: {order.DbKey} - {order.OrderId}");

            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<Order>> GetOrders(Season? season = null)
        {
            _logger.LogWarning($"Getting Orders at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var orders = new List<Order>();

            var items = await GetData(IdPrefix, "AND PaidOn > ''", "ORDER BY PaidOn DESC");

            foreach (var item in items)
            {
                var order = new Order();

                orders.Add(getOrderFromDbItem(item));
            }

            if (season.HasValue)
            {
                return orders.Where(x => x.Season == season.Value).ToList();
            }
            else
            {
                return orders;
            }

        }

        public async Task<Order> GetOrder(string dbKey)
        {
            var orderItems = await GetData(IdPrefix, $"AND ItemName() = '{dbKey}'");

            if (orderItems.Count() != 1)
            {
                throw new Exception($"Could not locate order: {dbKey}");
            }

            return getOrderFromDbItem(orderItems.First());
        }

        private Order getOrderFromDbItem(Item item)
        {
            var order = new Order();

            order.DbKey = item.Name;

            foreach (var attribute in item.Attributes)
            {
                switch (attribute.Name)
                {
                    case "OrderId":
                        order.OrderId = Convert.ToInt32(attribute.Value);
                        break;

                    case "OrderType":
                        order.OrderType = (PaymentType)(Convert.ToInt32(attribute.Value));
                        break;

                    case "Description":
                        order.Description = attribute.Value;
                        break;

                    case "TicketNumber":
                        order.TicketNumber = Convert.ToInt32(attribute.Value);
                        break;

                    case "MembersName":
                        order.MembersName = attribute.Value;
                        break;

                    case "Season":
                        order.SeasonName = attribute.Value;
                        break;

                    case "GuestsName":
                        order.GuestsName = attribute.Value;
                        break;

                    case "TicketHoldersName":
                        order.TicketHoldersName = attribute.Value;
                        break;

                    case "Amount":
                        order.Amount = decimal.Parse(attribute.Value);
                        break;

                    case "Fee":
                        order.Fee = decimal.Parse(attribute.Value);
                        break;

                    case "ValidOn":
                        order.ValidOn = attribute.Value != "" ? DateTime.Parse(attribute.Value) : null;
                        break;

                    case "PaidOn":
                        order.PaidOn = attribute.Value != "" ? DateTime.Parse(attribute.Value) : null;
                        break;

                    case "IssuedOn":
                        order.IssuedOn = attribute.Value != "" ? DateTime.Parse(attribute.Value) : null;
                        break;
                        
                    case "PaymentId":
                        order.PaymentId = attribute.Value;
                        break;

                    case "Status":
                        order.Status = attribute.Value;
                        break;

                    default:
                        break;
                }
            }

            return order;

        }

        public async Task DeleteOrder(string id)
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
