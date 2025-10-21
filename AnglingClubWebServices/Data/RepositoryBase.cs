using Amazon.S3;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public abstract class RepositoryBase
    {
        private RepositoryOptions _options;
        private ILogger<RepositoryBase> _logger;

        private const int QUERY_BATCH_SIZE = 2000;
        private const int UPDATE_BATCH_SIZE = 25;
        protected const string BACKUP_KEYNAME = "dbKey";

        private int batchNumber = 0;

        protected const string MultiValueSeparator = "~|";
        protected const int MultiValueSegmentSize = 1000;

        public RepositoryBase(RepositoryOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<RepositoryBase>();
            
            if (!checkDomainExists(_options.SimpleDbDomain).Result)
            {
                createDomain(_options.SimpleDbDomain).Wait();
            }

        }

        internal string Domain { get { return _options.SimpleDbDomain; } }

        private async Task createDomain(string domainName)
        {
            _logger.LogDebug($"Creating domain {domainName}");

            var client = GetClient();

            CreateDomainRequest request = new CreateDomainRequest(domainName);

            CreateDomainResponse response = await client.CreateDomainAsync(request);

            _logger.LogDebug("createDomain returned");
            _logger.LogDebug(response.ToString());

        }

        private async Task<bool> checkDomainExists(string domainName)
        {
            _logger.LogDebug($"Checking for domain   {domainName}");

            var client = GetClient();

            ListDomainsRequest request = new ListDomainsRequest();

            ListDomainsResponse response = await client.ListDomainsAsync(request);

            _logger.LogDebug(response.ToString());

            return response.DomainNames.Any(n => n == domainName);
        }

        internal async Task<int> GetNextId()
        {
            var client = GetClient();
            int nextId = -1;

            GetAttributesRequest request = new GetAttributesRequest();
            request.DomainName = _options.SimpleDbDomain;
            request.ItemName = "NextId";
            request.AttributeNames = new List<string> { "Id" };
            request.ConsistentRead = true;

            try
            {
                GetAttributesResponse response = await client.GetAttributesAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.Attributes.Count > 0)
                    {
                        nextId = Convert.ToInt32(response.Attributes[0].Value);
                    }

                    PutAttributesRequest putRequest = new PutAttributesRequest();
                    putRequest.DomainName = _options.SimpleDbDomain;
                    putRequest.ItemName = "NextId";
                    putRequest.Attributes.Add(
                        new ReplaceableAttribute { Name = "Id", Value = numberToString(++nextId), Replace = true }
                    );

                    try
                    {
                        PutAttributesResponse putResponse = await client.PutAttributesAsync(putRequest);
                    }
                    catch (AmazonSimpleDBException ex)
                    {
                        _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                        throw;
                    }
                }

            }
            catch (System.Exception)
            {

                throw;
            }

            return nextId;
        }

        internal AmazonSimpleDBClient GetClient()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            return client;
        }

        internal string numberToString(int number)
        {
            var numString = number.ToString("0000000000");

            return numString;
        }

        internal string pointsToString(float number)
        {
            var numString = number.ToString("000.000");

            return numString;
        }

        internal string weightToString(float number)
        {
            var numString = number.ToString("0000.0000");

            return numString;
        }

        internal string dateToString(DateTime date)
        {
            //return date.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        internal string dateOffsetToString(DateTimeOffset date)
        {
            //return date.ToString("yyyy-MM-ddTHH:mm:ss.000Z");
            return date.ToString("yyyy-MM-dd HH:mm:ss.000Z");
        }

        protected async Task<List<Item>> GetData(string idPrefix, string additionalWhereClause = "", string orderByClause = "")
        {
            List<Item> items = new List<Item>();

            var client = GetClient();
            string nextToken = null;

            do
            {
                SelectRequest request = new SelectRequest();
                request.SelectExpression = $"SELECT * FROM {Domain} WHERE ItemName() LIKE '{(idPrefix == "" ? "" : $"{idPrefix}:")}%' {additionalWhereClause} {orderByClause} LIMIT {QUERY_BATCH_SIZE}";
                request.NextToken = nextToken;

                SelectResponse response = await client.SelectAsync(request);
                nextToken = response.NextToken;

                items.AddRange(response.Items);

            } while (nextToken != null);

            return items;
        }

        protected async Task WriteInBatches(BatchPutAttributesRequest request, AmazonSimpleDBClient client)
        {
            List<ReplaceableAttribute> attributes = new List<ReplaceableAttribute>();

            request.Items = request.Items.OrderBy(x => x.Name).ToList();

            while (request.Items.Any())
            {
                batchNumber++;

                BatchPutAttributesRequest requestBatch = new BatchPutAttributesRequest();
                requestBatch.DomainName = request.DomainName;

                requestBatch.Items.AddRange(request.Items.Take(UPDATE_BATCH_SIZE));

                await storeBatchOfItems(client, requestBatch);

                request.Items.RemoveRange(0, request.Items.Count() < UPDATE_BATCH_SIZE ? request.Items.Count() : UPDATE_BATCH_SIZE);
            }

            // If backup is older than today - generate a new backup file
            var systemData = new SystemData();

            var items = await GetData("System");

            if (items.Any())
            {
                var item = items.Single();
                systemData.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "LastBackUp":
                            systemData.LastBackUp = DateTime.Parse(attribute.Value);
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                systemData.LastBackUp = DateTime.MinValue;
            }

            if (systemData.LastBackUp < DateTime.Now.AddDays(-1))
            {
                var backupData = await BackupData(-1);

                await saveBackup(backupData);

                try
                {
                    systemData.LastBackUp = DateTime.Now;

                    if (systemData.IsNewItem)
                    {
                        systemData.DbKey = systemData.GenerateDbKey("System");
                    }

                    BatchPutAttributesRequest sysDataRequest = new BatchPutAttributesRequest();
                    sysDataRequest.DomainName = Domain;

                    // Mandatory properties
                    var sysDataAttributes = new List<ReplaceableAttribute>
                    {
                        new ReplaceableAttribute { Name = "LastBackUp", Value = dateToString(systemData.LastBackUp), Replace = true },

                    };

                    sysDataRequest.Items.Add(
                        new ReplaceableItem
                        {
                            Name = systemData.DbKey,
                            Attributes = sysDataAttributes
                        }
                    );

                    try
                    {
                        await storeBatchOfItems(client, sysDataRequest);

                        _logger.LogDebug($"Sytem Data added: {systemData.DbKey}");
                    }
                    catch (AmazonSimpleDBException ex)
                    {
                        _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to store System data");
                    throw;
                }

            }
        }

        protected async Task<List<BackupLine>> BackupData(int itemsToBackup)
        {
            _logger.LogWarning($"Getting backup items at : {DateTime.Now.ToString("HH:mm:ss.000")}");

            var backupLines = new List<BackupLine>();
            var lineNo = 0;
            var itemsBackedUp = 0;

            var items = await GetData("");

            foreach (var item in items)
            {
                if (itemsToBackup == -1 || itemsBackedUp < itemsToBackup)
                {
                    backupLines.Add(new BackupLine { LineNumber = lineNo++, AttributeName = BACKUP_KEYNAME, AttributeValue = item.Name });
                    itemsBackedUp++;

                    foreach (var attribute in item.Attributes)
                    {
                        backupLines.Add(new BackupLine { LineNumber = lineNo++, AttributeName = attribute.Name, AttributeValue = attribute.Value });
                    }
                }
            }

            return backupLines;

        }

        protected MultiValued GetMultiValuedElement(string attributeValue)
        {
            if (attributeValue.Contains(MultiValueSeparator))
            {
                return new MultiValued { Index = Convert.ToInt32(attributeValue.Split(MultiValueSeparator)[0]), Text = attributeValue.Split(MultiValueSeparator)[1] };
            }
            else
            {
                return new MultiValued { Index = 0, Text = attributeValue };
            }
        }

        protected class MultiValued
        {
            public int Index { get; set; }
            public string Text { get; set; }
        }


        private async Task storeBatchOfItems(AmazonSimpleDBClient client, BatchPutAttributesRequest request)
        {
            try
            {
                /*
                _logger.LogDebug($"Writing new batch {batchNumber}...");
                var itemNo = 1;

                foreach (var item in request.Items)
                {
                    _logger.LogDebug($"  Item: {itemNo++} = {item.Name}");
                }
                */

                BatchPutAttributesResponse response = await client.BatchPutAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        private async Task saveBackup(List<BackupLine> backupData)
        {
            var backupAsString = JsonConvert.SerializeObject(backupData);

            AmazonS3Client s3Client;

            s3Client = new AmazonS3Client(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest 
            {
                BucketName = _options.BackupBucket,
                Key = $"Backup_{Domain}_{DateTime.Now:yyyy-MM-dd}.json",
                ContentType = "application/json",
                ContentBody = backupAsString

            });

        }

        protected async Task saveBase64AsFile(string fileContents, string fileName)
        {
            AmazonS3Client s3Client;

            s3Client = new AmazonS3Client(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = _options.TmpFilesBucket,
                Key = fileName,
                ContentType = "text/plain",
                ContentBody = fileContents

            });

        }

        protected async Task<string> getFileAsBase64(string fileName)
        {
            AmazonS3Client s3Client;

            s3Client = new AmazonS3Client(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            var fileContents = await s3Client.GetObjectAsync(new Amazon.S3.Model.GetObjectRequest
            {
                BucketName = _options.TmpFilesBucket,
                Key = fileName
            });

            StreamReader reader = new StreamReader(fileContents.ResponseStream);

            String content = reader.ReadToEnd();

            return content;
        }

        protected async Task deleteFile(string fileName)
        {
            AmazonS3Client s3Client;
            s3Client = new AmazonS3Client(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));
            await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest
            {
                BucketName = _options.TmpFilesBucket,
                Key = fileName
            });
        }

    }
}
