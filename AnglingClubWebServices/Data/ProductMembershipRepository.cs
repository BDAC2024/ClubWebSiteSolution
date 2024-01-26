using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Helpers;
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
    public class ProductMembershipRepository : RepositoryBase, IProductMembershipRepository
    {
        private const string IdPrefix = "ProductMembership";
        private readonly ILogger<ProductMembershipRepository> _logger;


        public ProductMembershipRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProductMembershipRepository>();
            SiteUrl = opts.Value.SiteUrl;
        }

        public string SiteUrl { get; }

        public async Task AddOrUpdateProductMembership(ProductMembership membership)
        {
            var client = GetClient();

            if (membership.IsNewItem)
            {
                membership.DbKey = membership.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Type", Value = ((int)membership.Type).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Description", Value = membership.Description, Replace = true },
                new ReplaceableAttribute { Name = "Term", Value = membership.Term, Replace = true },
                new ReplaceableAttribute { Name = "Runs", Value = membership.Runs, Replace = true },
                new ReplaceableAttribute { Name = "Cost", Value = membership.Cost.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "PriceId", Value = membership.PriceId, Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = membership.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"ProductMembership added: {membership.DbKey} - {membership.Type.EnumDescription()}");

            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<ProductMembership>> GetProductMemberships()
        {
            _logger.LogWarning($"Getting ProductMemberships at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var memberships = new List<ProductMembership>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var membership = new ProductMembership();

                membership.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Type":
                            membership.Type = (MembershipType)(Convert.ToInt32(attribute.Value)); ;
                            break;

                        case "Description":
                            membership.Description = attribute.Value;
                            break;

                        case "Term":
                            membership.Term = attribute.Value;
                            break;

                        case "Runs":
                            membership.Runs = attribute.Value;
                            break;

                        case "Cost":
                            membership.Cost = decimal.Parse(attribute.Value);
                            break;

                        case "PriceId":
                            membership.PriceId = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                memberships.Add(membership);
            }


            return memberships.ToList();
        }

        public async Task DeleteProductMembership(string id)
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
