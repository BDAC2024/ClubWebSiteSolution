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
    public class AppSettingsRepository : RepositoryBase, IAppSettingsRepository
    {
        private const string IdPrefix = "AppSettings";
        private readonly ILogger<AppSettingsRepository> _logger;

        public AppSettingsRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppSettingsRepository>();
        }

        public async Task AddOrUpdateAppSettings(AppSettings appSettings)
        {
            var client = GetClient();

            if (appSettings.IsNewItem)
            {
                appSettings.DbKey = appSettings.GenerateDbKey(IdPrefix);
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "GuestTicketCost", Value = appSettings.GuestTicketCost.ToString(), Replace = true },
            };

            foreach (var previewer in appSettings.Previewers)
            {
                attributes.Add(new ReplaceableAttribute { Name = "Previewers", Value = previewer.ToString(), Replace = true });
            }

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = appSettings.DbKey,
                    Attributes = attributes
                }
            );

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"App Settings added: {appSettings.DbKey}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

        }

        public async Task<AppSettings> GetAppSettings()
        {
            _logger.LogWarning($"Getting appSettings at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var appSettings = new AppSettings();

            var item = (await GetData(IdPrefix)).First();

            appSettings.DbKey = item.Name;

            foreach (var attribute in item.Attributes)
            {
                switch (attribute.Name)
                {
                    case "GuestTicketCost":
                        appSettings.GuestTicketCost = Decimal.Parse(attribute.Value);
                        break;


                    case "Previewers":
                        appSettings.Previewers.Add(Convert.ToInt32(attribute.Value));
                        break;

                    default:
                        break;
                }
            }

            return appSettings;

        }

        public async Task DeleteAppSettings(string id)
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
