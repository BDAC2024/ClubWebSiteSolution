using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        internal AmazonS3Client GetS3Client()
        {
            AmazonS3Client client = new AmazonS3Client(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            return client;
        }


        internal AmazonSimpleDBClient GetClient()
        {
            AmazonSimpleDBClient client = new AmazonSimpleDBClient(_options.AWSAccessId, _options.AWSSecret, Amazon.RegionEndpoint.GetBySystemName(_options.AWSRegion));

            return client;
        }

        internal void SetupTableAttribues(BatchPutAttributesRequest request, string dbKey, List<ReplaceableAttribute> attributes)
        {
            request.Items = new List<ReplaceableItem>();
            request.Items.Add(new ReplaceableItem
            {
                Name = dbKey,
                Attributes = attributes
            });
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

                if (response.Items != null && response.Items.Count > 0)
                {
                    items.AddRange(response.Items);
                }

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

                _logger.LogInformation($"Restoring batch: {batchNumber}, done {(batchNumber - 1) * UPDATE_BATCH_SIZE} items so far");
                BatchPutAttributesRequest requestBatch = new BatchPutAttributesRequest();
                requestBatch.DomainName = request.DomainName;

                requestBatch.Items = new List<ReplaceableItem>();
                requestBatch.Items.AddRange(request.Items.Take(UPDATE_BATCH_SIZE));

                await storeBatchOfItems(client, requestBatch);

                request.Items.RemoveRange(0, request.Items.Count() < UPDATE_BATCH_SIZE ? request.Items.Count() : UPDATE_BATCH_SIZE);
            }

            if (_options.Stage.ToLower() != "prod")
            {
                _logger.LogWarning($"Backup disabled as running in {_options.Stage}");
                return;
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

                    SetupTableAttribues(sysDataRequest, systemData.DbKey, sysDataAttributes);

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

            using (var s3Client = GetS3Client())
            {
                await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = _options.BackupBucket,
                    Key = $"Backup_{Domain}_{DateTime.Now:yyyy-MM-dd}.json",
                    ContentType = "application/json",
                    ContentBody = backupAsString

                });
            }
        }

        protected async Task<MemoryStream> getFile(string fileName, string bucketName)
        {
            var ms = new MemoryStream();

            using (var s3Client = GetS3Client())
            {
                var fileContents = await s3Client.GetObjectAsync(new Amazon.S3.Model.GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                });

                using var responseStream = fileContents.ResponseStream;
                await responseStream.CopyToAsync(ms);

            }

            ms.Position = 0;
            return ms;
        }

        protected async Task<string> getFileAsBase64(string fileName, string bucketName)
        {
            var ms = await getFile(fileName, bucketName);

            var bytes = ms.ToArray();
            var contentBase64 = Convert.ToBase64String(bytes);

            return contentBase64;
        }

        protected async Task<List<StoredFileMeta>> getFilesFromS3(string bucketName)
        {
            var results = new List<StoredFileMeta>();

            using (var s3Client = GetS3Client())
            {

                try
                {
                    string continuationToken = null;

                    do
                    {
                        var listRequest = new ListObjectsV2Request
                        {
                            BucketName = bucketName,
                            ContinuationToken = continuationToken,
                            MaxKeys = 1000
                        };

                        var listResponse = await s3Client.ListObjectsV2Async(listRequest);

                        foreach (var s3Object in listResponse.S3Objects)
                        {
                            try
                            {
                                // Get metadata for each object
                                var metaResponse = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                                {
                                    BucketName = bucketName,
                                    Key = s3Object.Key
                                });

                                var meta = new
                                {
                                    Key = s3Object.Key,
                                    Size = metaResponse.ContentLength,
                                    LastModified = metaResponse.LastModified,
                                    ContentType = metaResponse.Headers.ContentType,
                                    ETag = metaResponse.ETag,
                                    UserMetadata = metaResponse.Metadata // IDictionary<string,string>
                                };

                                var storedFileMeta = new StoredFileMeta
                                {
                                    Id = s3Object.Key,
                                    Created = metaResponse.LastModified.Value,
                                };

                                results.Add(storedFileMeta);
                            }
                            catch (AmazonS3Exception s3Ex)
                            {
                                _logger.LogError(s3Ex, $"Failed to get metadata for S3 object {s3Object.Key} in bucket {bucketName}");
                                // skip problematic object but continue
                            }
                        }

                        continuationToken = listResponse.IsTruncated.Value ? listResponse.NextContinuationToken : null;

                    } while (continuationToken != null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to list objects in bucket {bucketName}");
                    throw;
                }
            }

            return results;
        }

        protected async Task deleteFile(string fileName, string bucketName)
        {
            bool exists;

            using (var s3Client = GetS3Client())
            {
                try
                {
                    await s3Client.GetObjectMetadataAsync(bucketName, fileName);
                    exists = true;
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    exists = false;
                    throw new Exception($"Unable to delete {fileName}, it does not exist");
                }

                if (exists)
                {
                    {
                        await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest
                        {
                            BucketName = bucketName,
                            Key = fileName
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Initial attempts to upload the file as an arg to a web api call failed on AWS with a 413 (content too large) error.
        /// The approach here is to get a pre-signed URL from the web api, then use that URL to upload the file directly to S3.
        /// The solution was obtained from ChatGPT
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="contentType"></param>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        protected async Task<string> getPreSignedUploadUrl(string filename, string contentType, string bucketName)
        {
            string url = null;

            using (var s3Client = GetS3Client())
            {
                var requestModel = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = filename,
                    Verb = HttpVerb.PUT,
                    ContentType = contentType,
                    Expires = DateTime.UtcNow.AddMinutes(5)
                };

                url = await s3Client.GetPreSignedURLAsync(requestModel);
            }

            return url;
        }

        /// <summary>
        /// Store a file in an S3 bucket
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileBytes"></param>
        /// <param name="contentType"></param>
        /// <param name="bucketName"></param>
        /// <param name="useEncryption"></param>
        /// <returns></returns>
        protected async Task saveFile(string fileName, byte[] fileBytes, string contentType, string bucketName, bool useEncryption = true)
        {
            using (var s3Client = GetS3Client())
            {

                var requestModel = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    ContentType = contentType,
                    InputStream = new MemoryStream(fileBytes)
                };

                if (useEncryption)
                {
                    requestModel.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                }

                await s3Client.PutObjectAsync(requestModel);
            }
        }

        /// <summary>
        /// Get a short-lived presigned URL to download a file from S3
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bucketName"></param>
        /// <param name="minutesBeforeExpiry"></param>
        /// <param name="downloadAs"></param>
        /// <param name="returnedFileName">Optional: name of file sent back to browser</param>
        /// <param name="contentType">Optional: mime type of file</param>
        /// <returns></returns>
        protected string getFilePresignedUrl(string fileName, string bucketName, int minutesBeforeExpiry, DownloadType downloadAs, string returnedFileName = "", string contentType = "")
        {
            string presignedUrl = null;

            using (var s3Client = GetS3Client())
            {
                var headers = new ResponseHeaderOverrides();

                if (contentType != "")
                {
                    headers.ContentType = contentType;
                }
                headers.ContentDisposition = $"{downloadAs.ToString()}; filename={(returnedFileName != "" ? returnedFileName : fileName)}";

                presignedUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    Expires = DateTime.UtcNow.AddMinutes(minutesBeforeExpiry),
                    Verb = HttpVerb.GET,
                    ResponseHeaderOverrides = headers
                });

            }

            return presignedUrl;
        }
    }
}
