using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class OpenMatchRegistrationRepository : RepositoryBase, IOpenMatchRegistrationRepository
    {
        private const string IdPrefix = "OpenMatchRegistration";
        private readonly ILogger<OpenMatchRegistrationRepository> _logger;

        public OpenMatchRegistrationRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OpenMatchRegistrationRepository>();
        }

        public async Task AddOrUpdateOpenMatchRegistration(OpenMatchRegistration registration)
        {
            var client = GetClient();

            if (registration.IsNewItem)
            {
                registration.DbKey = registration.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties 
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "OpenMatchId", Value = registration.OpenMatchId, Replace = true },
                new ReplaceableAttribute { Name = "RegistrationNumber", Value = ((int)registration.RegistrationNumber).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Name", Value = registration.Name, Replace = true },
                new ReplaceableAttribute { Name = "AgeGroup", Value = ((int)registration.AgeGroup).ToString(), Replace = true },
                new ReplaceableAttribute { Name = "Address", Value = registration.Address, Replace = true },
                new ReplaceableAttribute { Name = "ParentName", Value = registration.ParentName, Replace = true },
                new ReplaceableAttribute { Name = "EmergencyContactPhone", Value = registration.EmergencyContactPhone, Replace = true },
            };

            if (!string.IsNullOrEmpty(registration.ContactEmail))
            {
                attributes.Add(
                    new ReplaceableAttribute { Name = "ContactEmail", Value = registration.ContactEmail, Replace = true }
                );
            }

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = registration.DbKey,
                    Attributes = attributes
                }
            );

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"Open Match Registration added: {registration.DbKey} - {registration.Name}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        public async Task<List<OpenMatchRegistration>> GetOpenMatchRegistrations()
        {
            _logger.LogWarning($"Getting Open Match Registrations at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var openMatcheRegistrations = new List<OpenMatchRegistration>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var openMatchRegistration = new OpenMatchRegistration();

                openMatchRegistration.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "OpenMatchId":
                            openMatchRegistration.OpenMatchId = attribute.Value;
                            break;

                        case "RegistrationNumber":
                            openMatchRegistration.RegistrationNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "Name":
                            openMatchRegistration.Name = attribute.Value;
                            break;

                        case "AgeGroup":
                            openMatchRegistration.AgeGroup = (JuniorAgeGroup)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Address":
                            openMatchRegistration.Address = attribute.Value;
                            break;

                        case "ParentName":
                            openMatchRegistration.ParentName = attribute.Value;
                            break;

                        case "EmergencyContactPhone":
                            openMatchRegistration.EmergencyContactPhone = attribute.Value;
                            break;

                        case "ContactEmail":
                            openMatchRegistration.ContactEmail = attribute.Value;
                            break;


                        default:
                            break;
                    }
                }

                openMatcheRegistrations.Add(openMatchRegistration);
            }

            return openMatcheRegistrations;

        }

        public async Task DeleteOpenMatchRegistration(string id)
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
