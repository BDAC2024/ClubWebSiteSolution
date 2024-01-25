using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnglingClubWebServices.Data
{
    public class AppSettingRepository : RepositoryBase, IAppSettingRepository
    {
        private const string IdPrefix = "AppSetting";
        private readonly ILogger<AppSettingRepository> _logger;

        public AppSettingRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppSettingRepository>();
        }

        public async Task AddOrUpdateAppSettings(AppSettings appSettings)
        {
            await AddOrUpdateAppSetting(new AppSetting { Name = "GuestTicketCost", Value = appSettings.GuestTicketCost.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicketCost", Value = appSettings.DayTicketCost.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicketStyle", Value = appSettings.DayTicketStyle });
            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicket", Value = appSettings.DayTicket });
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductDayTicket", Value = appSettings.ProductDayTicket });
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductGuestTicket", Value = appSettings.ProductGuestTicket });

            await AddOrUpdateAppSetting(new AppSetting { Name = "Previewers", Value = String.Join(",", appSettings.Previewers.ToArray()) });

        }

        public async Task AddOrUpdateAppSetting(AppSetting appSetting)
        {
            var client = GetClient();

            var existingItem = (await getAppSettings()).FirstOrDefault(x => x.Name == appSetting.Name);

            if (existingItem == null)
            {
                appSetting.DbKey = appSetting.GenerateDbKey(IdPrefix);
            }
            else
            {
                appSetting.DbKey = existingItem.DbKey;
            }

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            // Mandatory properties
            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Name", Value = appSetting.Name, Replace = true },
                new ReplaceableAttribute { Name = "Value", Value = appSetting.Value, Replace = true },
            };

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = appSetting.DbKey,
                    Attributes = attributes
                }
            );

            try
            {
                //BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
                await WriteInBatches(request, client);
                _logger.LogDebug($"App Setting added: {appSetting.DbKey}");
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

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var settingName = "";
                var settingValue = "";

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Name":
                            settingName = attribute.Value;
                            break;

                        case "Value":
                            settingValue = attribute.Value;
                            break;

                        default:
                            break;
                    }

                }

                switch (settingName.ToLower())
                {
                    case "guestticketcost":
                        appSettings.GuestTicketCost = Convert.ToDecimal(settingValue);
                        break;

                    case "dayticketcost":
                        appSettings.DayTicketCost = Convert.ToDecimal(settingValue);
                        break;

                    case "dayticketstyle":
                        appSettings.DayTicketStyle = settingValue;
                        break;

                    case "dayticket":
                        appSettings.DayTicket = settingValue;
                        break;

                    case "productdayticket":
                        appSettings.ProductDayTicket = settingValue;
                        break;

                    case "productguestticket":
                        appSettings.ProductGuestTicket = settingValue;
                        break;

                    case "previewers":
                        foreach (var previewer in settingValue.Split(","))
                        {
                            appSettings.Previewers.Add(Convert.ToInt32(previewer));
                        }
                        
                        break;

                    default:
                        break;
                }

            }

            return appSettings;

        }

        public async Task DeleteAppSetting(string id)
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

        private async Task<List<AppSetting>> getAppSettings()
        {
            _logger.LogWarning($"Getting appSettings at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var appSettings = new List<AppSetting>();

            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var appSetting = new AppSetting();

                appSetting.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Name":
                            appSetting.Name = attribute.Value;
                            break;

                        case "Value":
                            appSetting.Value = attribute.Value;
                            break;

                        default:
                            break;
                    }

                }

                appSettings.Add(appSetting);
            }

            return appSettings;

        }
    }
}
