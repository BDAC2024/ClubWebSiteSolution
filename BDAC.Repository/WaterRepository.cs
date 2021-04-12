using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;

namespace BDAC.Repository
{
    public class WaterRepository : RepositoryBase
    {

        public WaterRepository(): base()
        {
        }

        public async Task AddOrUpdateWater()
        {
            var client = GetClient();

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = DOMAIN;
            request.Items.Add(
                new ReplaceableItem
                {
                    Name = "Roecliffe Pond",
                    Attributes = new List<ReplaceableAttribute>
                    {
                        new ReplaceableAttribute { Name = "Type", Value = "Water", Replace = false },
                        new ReplaceableAttribute { Name = "Id", Value = "1", Replace = false },
                        new ReplaceableAttribute { Name = "AccessType", Value = "MembersAndGuests", Replace = false },
                        new ReplaceableAttribute { Name = "WaterType", Value = "Stillwater", Replace = false },
                    }
                }
            );

            try
            {
                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                Console.WriteLine($"Water updated");
            }
            catch (AmazonSimpleDBException ex)
            {
                Console.WriteLine($"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

            
    }
}
