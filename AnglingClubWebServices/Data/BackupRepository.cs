using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class BackupRepository : RepositoryBase, IBackupRepository
    {
        private readonly ILogger<BackupRepository> _logger;
        private readonly IOptions<DeploymentOptions> _deployment_options;
        public BackupRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory,
            IOptions<DeploymentOptions> deployment_options) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BackupRepository>();
            _deployment_options = deployment_options;
        }

        public async Task Restore(List<BackupLine> backupLines, string restoreToDomain)
        {
            var client = GetClient();

            if (await DbNotEmpty(restoreToDomain, client))
            {
                var ex = new Exception($"Cannot restore to {restoreToDomain} because it is not empty!");
                _logger.LogError(ex, ex.Message);
                throw ex;
            }

            var lineCount = backupLines.OrderByDescending(x => x.LineNumber).First().LineNumber;
            var newItem = true;

            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = restoreToDomain;
            List<ReplaceableAttribute> attributes = new List<ReplaceableAttribute>();
            ReplaceableItem item = null;
            request.Items = new List<ReplaceableItem>();

            foreach (var line in backupLines.OrderBy(x => x.LineNumber))
            {
                newItem = line.AttributeName == BACKUP_KEYNAME;

                if (newItem)
                {
                    if (item != null && attributes.Any())
                    {
                        item.Attributes = attributes;
                        request.Items.Add(item);
                        attributes = new List<ReplaceableAttribute>();
                    }

                    item = new ReplaceableItem
                    {
                        Name = line.AttributeValue
                    };
                }
                else
                {
                    attributes.Add(new ReplaceableAttribute { Name = line.AttributeName, Value = line.AttributeValue, Replace = true });
                }

                if (line.LineNumber == lineCount)
                {
                    item.Attributes = attributes;
                    request.Items.Add(item);
                }

            }

            await WriteInBatches(request, client);

            _logger.LogDebug($"Restore completed");

        }

        public async Task<List<BackupLine>> Backup(int itemsToBackup)
        {
            _logger.LogWarning($"Getting backup items at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            return await BackupData(itemsToBackup);
        }

        public async Task ClearDb(string domainToClear)
        {
            var client = GetClient();

            var itemNames = Backup(-1).Result.Where(x => x.AttributeName == BACKUP_KEYNAME).Select(x => x.AttributeValue);
            var batch = 0; // Max 25 delets per batch
            List<string> itemBatch;

            do
            {
                itemBatch = itemNames.Skip(batch).Take(25).ToList();

                if (itemBatch.Any())
                {
                    BatchDeleteAttributesRequest request = new BatchDeleteAttributesRequest();
                    request.Items = new List<DeletableItem>();

                    request.DomainName = domainToClear;

                    foreach (var itemName in itemBatch)
                    {
                        request.Items.Add(new DeletableItem { Name = itemName });
                    }

                    try
                    {
                        BatchDeleteAttributesResponse response = await client.BatchDeleteAttributesAsync(request);
                        batch++;
                    }
                    catch (AmazonSimpleDBException ex)
                    {
                        _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                        throw;
                    }
                }
            } while (itemBatch.Any());

        }

        private async Task<bool> DbNotEmpty(string domainToCheck, AmazonSimpleDBClient client)
        {
            SelectRequest request = new SelectRequest();
            request.SelectExpression = $"SELECT count(*) FROM `{domainToCheck}`";

            SelectResponse response = await client.SelectAsync(request);

            return response.Items.First().Attributes.First().Value != "0";
        }
    }
}
