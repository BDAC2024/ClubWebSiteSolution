using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Enums;
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
    public class TmpFileRepository : RepositoryBase, ITmpFileRepository
    {
        private const string IdPrefix = "TmpFile";
        private const int DAYS_TO_EXPIRE = 7; // Will purge files older than this
        private readonly ILogger<TmpFileRepository> _logger;
        private readonly RepositoryOptions _options;

        public TmpFileRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _options = opts.Value;
            _logger = loggerFactory.CreateLogger<TmpFileRepository>();
        }

        public async Task AddOrUpdateTmpFile(StoredFileMeta file)
        {
            var client = GetClient();

            if (string.IsNullOrEmpty(file.Id))
                throw new ArgumentException("TmpFile Id cannot be null or empty.");

            // Store the Id as a main attribute
            BatchPutAttributesRequest request = new BatchPutAttributesRequest();
            request.DomainName = Domain;

            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute { Name = "Id", Value = file.Id, Replace = true },
                new ReplaceableAttribute { Name = "Created", Value = dateToString(DateTime.Now), Replace = true }
            };

            base.SetupTableAttribues(request, $"{IdPrefix}:{file.Id}", attributes);

            try
            {
                await WriteInBatches(request, client);
                _logger.LogDebug($"TmpFile meta added/updated: {file.Id}");
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }

            await PurgeTmpFiles();
        }


        /// <summary>
        /// Loads all TmpFiles - optionally loading the file content
        /// </summary>
        /// <param name="loadFile"></param>
        /// <returns></returns>
        public async Task<List<StoredFile>> GetTmpFiles(bool loadFile = false)
        {
            var files = new List<StoredFile>();
            var items = await GetData(IdPrefix, "AND Id > ''", "ORDER BY Id");

            foreach (var item in items)
            {
                try
                {
                    files.Add(await GetTmpFileFromDbItem(item, loadFile));
                }
                catch (Exception)
                {
                    // File is listed in SimpleDB but not found in S3
                    files.Add(new StoredFile
                    {
                        Id = item.Attributes.FirstOrDefault(a => a.Name == "Id")?.Value,
                        Created = DateTime.Parse(item.Attributes.FirstOrDefault(a => a.Name == "Created")?.Value ?? DateTime.MinValue.ToString()),
                    });
                }
            }

            return files;
        }

        public async Task<StoredFile> GetTmpFile(string id)
        {
            var items = await GetData(IdPrefix, $"AND ItemName() = '{IdPrefix}:{id}'");
            if (items.Count != 1)
                throw new Exception($"Could not locate TmpFile: {id}");

            try
            {
                return await GetTmpFileFromDbItem(items.First(), true);
            }
            catch (Exception)
            {
                // File is listed in SimpleDB but not found in S3
                return new StoredFile
                {
                    Id = items.First().Attributes.FirstOrDefault(a => a.Name == "Id")?.Value,
                    Created = DateTime.Parse(items.First().Attributes.FirstOrDefault(a => a.Name == "Created")?.Value ?? DateTime.MinValue.ToString()),
                };
            }
        }

        public async Task DeleteTmpFile(string id, bool deleteFromS3 = true)
        {
            var client = GetClient();

            DeleteAttributesRequest request = new DeleteAttributesRequest
            {
                DomainName = Domain,
                ItemName = $"{IdPrefix}:{id}"
            };

            try
            {
                if (deleteFromS3)
                {
                    await base.deleteFile(id, _options.TmpFilesBucket);
                }
                await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        private async Task<StoredFile> GetTmpFileFromDbItem(Item item, bool loadFile)
        {
            var file = new StoredFile();
            var contentArr = new List<MultiValued>();

            foreach (var attribute in item.Attributes)
            {
                switch (attribute.Name)
                {
                    case "Id":
                        file.Id = attribute.Value;
                        break;
                    case "Created":
                        file.Created = DateTime.Parse(attribute.Value);
                        break;
                }
            }

            if (loadFile)
            {
                file.Content = await base.getFileAsBase64(file.Id, _options.TmpFilesBucket);
            }

            return file;
        }

        public async Task PurgeTmpFiles()
        {
            var tmpFiles = await GetTmpFiles();

            //var expiryTime = DateTime.Now.AddMinutes(-1);
            var expiryTime = DateTime.Now.AddDays(-DAYS_TO_EXPIRE);

            foreach (var file in tmpFiles)
            {
                if (file.Created.ToUniversalTime() < expiryTime.ToUniversalTime())
                {
                    _logger.LogInformation($"Purging TmpFile: {file.Id}, Created: {file.Created}");
                    await DeleteTmpFile(file.Id);
                }
            }

            // Now remove any tmp files that didn't have a corresponding Db entry
            var tmpFilesFromS3 = await getFilesFromS3(_options.TmpFilesBucket);

            foreach (var file in tmpFilesFromS3)
            {
                if (file.Created.ToUniversalTime() < expiryTime.ToUniversalTime())
                {
                    _logger.LogInformation($"Purging S3 TmpFile: {file.Id}, Created: {file.Created}");
                    await base.deleteFile(file.Id, _options.TmpFilesBucket);
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
        /// <returns></returns>
        public async Task<string> GetTmpFileUploadUrl(string filename, string contentType)
        {
            return await base.getPreSignedUploadUrl(filename, contentType, _options.TmpFilesBucket);
        }

        public async Task SaveTmpFile(string fileName, byte[] fileBytes, string contentType)
        {
            await base.saveFile(fileName, fileBytes, contentType, _options.TmpFilesBucket, false);
        }

        public async Task<string> GetFilePresignedUrl(string fileName, int minutesBeforeExpiry, string contentType)
        {
            await Task.Delay(0);

            return base.getFilePresignedUrl(fileName, _options.TmpFilesBucket, minutesBeforeExpiry, DownloadType.inline);
        }
    }
}