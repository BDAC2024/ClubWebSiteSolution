using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class UserAdminRepository : RepositoryBase, IUserAdminRepository
    {
        private const string IdPrefix = "UserAdmin";
        private readonly ILogger<UserAdminRepository> _logger;

        public UserAdminRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UserAdminRepository>();
        }

        public async Task AddOrUpdateUserAdmin(UserAdminContact userAdmin)
        {
            var client = GetClient();

            if (userAdmin.IsNewItem)
            {
                userAdmin.DbKey = userAdmin.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "EmailAddress", Value = userAdmin.EmailAddress, Replace = true },

            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = userAdmin.DbKey,
                    Attributes = attributes
                }
            ); 

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"User admin added: {userAdmin.DbKey} - {userAdmin.EmailAddress}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<List<UserAdminContact>> GetUserAdmins()
        {
            _logger.LogWarning($"Getting user admins at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var userAdmins = new List<UserAdminContact>();

            var items = await GetData(IdPrefix, "AND EmailAddress > ''", "ORDER BY EmailAddress");

            foreach (var item in items)
            {
                var userAdmin = new UserAdminContact();

                userAdmin.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "EmailAddress":
                            userAdmin.EmailAddress = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                userAdmins.Add(userAdmin);
            }

            return userAdmins;

        }

        public async Task DeleteUserAdmin(string id)
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
