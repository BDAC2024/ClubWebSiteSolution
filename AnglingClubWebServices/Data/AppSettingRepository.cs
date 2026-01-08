using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Models;
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
            await AddOrUpdateAppSetting(new AppSetting { Name = "MembershipsEnabled", Value = appSettings.MembershipsEnabled.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "GuestTicketsEnabled", Value = appSettings.GuestTicketsEnabled.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicketsEnabled", Value = appSettings.DayTicketsEnabled.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "PondGateKeysEnabled", Value = appSettings.PondGateKeysEnabled.ToString() }); 

            await AddOrUpdateAppSetting(new AppSetting { Name = "GuestTicketCost", Value = appSettings.GuestTicketCost.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicketCost", Value = appSettings.DayTicketCost.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "PondGateKeyCost", Value = appSettings.PondGateKeyCost.ToString() });
            await AddOrUpdateAppSetting(new AppSetting { Name = "HandlingCharge", Value = appSettings.HandlingCharge.ToString() });
            
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductDayTicket", Value = appSettings.ProductDayTicket });
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductGuestTicket", Value = appSettings.ProductGuestTicket });
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductPondGateKey", Value = appSettings.ProductPondGateKey });
            await AddOrUpdateAppSetting(new AppSetting { Name = "ProductHandlingCharge", Value = appSettings.ProductHandlingCharge });
            
            await AddOrUpdateAppSetting(new AppSetting { Name = "Previewers", Value = String.Join(",", appSettings.Previewers.ToArray()) });
            await AddOrUpdateAppSetting(new AppSetting { Name = "MembershipSecretaries", Value = String.Join(",", appSettings.MembershipSecretaries.ToArray()) });
            await AddOrUpdateAppSetting(new AppSetting { Name = "Treasurers", Value = String.Join(",", appSettings.Treasurers.ToArray()) });
            await AddOrUpdateAppSetting(new AppSetting { Name = "CommitteeMembers", Value = String.Join(",", appSettings.CommitteeMembers.ToArray()) });
            await AddOrUpdateAppSetting(new AppSetting { Name = "Secretaries", Value = String.Join(",", appSettings.Secretaries.ToArray()) });

            await AddOrUpdateAppSetting(new AppSetting { Name = "DayTicketClosureTimesPerMonth", Value = appSettings.DayTicketClosureTimesPerMonth });
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

            base.SetupTableAttribues(request, appSetting.DbKey, attributes);

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
                    case "membershipsenabled":
                        appSettings.MembershipsEnabled = Convert.ToBoolean(settingValue);
                        break;

                    case "guestticketsenabled":
                        appSettings.GuestTicketsEnabled = Convert.ToBoolean(settingValue);
                        break;

                    case "dayticketsenabled":
                        appSettings.DayTicketsEnabled = Convert.ToBoolean(settingValue); 
                        break;

                    case "pondgatekeysenabled":
                        appSettings.PondGateKeysEnabled = Convert.ToBoolean(settingValue); 
                        break;

                    case "guestticketcost":
                        appSettings.GuestTicketCost = Convert.ToDecimal(settingValue);
                        break;

                    case "dayticketcost":
                        appSettings.DayTicketCost = Convert.ToDecimal(settingValue);
                        break;

                    case "pondgatekeycost":
                        appSettings.PondGateKeyCost = Convert.ToDecimal(settingValue);
                        break;

                    case "handlingcharge":
                        appSettings.HandlingCharge = Convert.ToDecimal(settingValue);
                        break;
                        
                    case "productdayticket":
                        appSettings.ProductDayTicket = settingValue;
                        break;

                    case "productguestticket":
                        appSettings.ProductGuestTicket = settingValue;
                        break;

                    case "productpondgatekey":
                        appSettings.ProductPondGateKey = settingValue;
                        break;

                    case "producthandlingcharge":
                        appSettings.ProductHandlingCharge = settingValue;
                        break;

                    case "dayticketclosuretimespermonth":
                        appSettings.DayTicketClosureTimesPerMonth = settingValue;
                        break;

                        
                    case "previewers":
                        if (settingValue != "")
                        {
                            foreach (var previewer in settingValue.Split(","))
                            {
                                appSettings.Previewers.Add(Convert.ToInt32(previewer));
                            }
                        }
                        break;

                    case "membershipsecretaries":
                        if (settingValue != "")
                        {
                            foreach (var membershipSecretary in settingValue.Split(","))
                            {
                                appSettings.MembershipSecretaries.Add(Convert.ToInt32(membershipSecretary));
                            }
                        }
                        break;
                        
                    case "treasurers":
                        if (settingValue != "")
                        {
                            foreach (var treasurer in settingValue.Split(","))
                            {
                                appSettings.Treasurers.Add(Convert.ToInt32(treasurer));
                            }
                        }
                        break;

                    case "committeemembers":
                        if (settingValue != "")
                        {
                            foreach (var committeeMember in settingValue.Split(","))
                            {
                                appSettings.CommitteeMembers.Add(Convert.ToInt32(committeeMember));
                            }
                        }
                        break;

                    case "secretaries":
                        if (settingValue != "")
                        {
                            foreach (var secretary in settingValue.Split(","))
                            {
                                appSettings.Secretaries.Add(Convert.ToInt32(secretary));
                            }
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
