using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class TmpFileRepository : RepositoryBase, ITmpFileRepository
    {
        private const string IdPrefix = "TmpFile";
        private const int DAYS_TO_EXPIRE = 7; // Will purge files older than this
        private readonly ILogger<TmpFileRepository> _logger;

        public TmpFileRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TmpFileRepository>();
        }

        public async Task AddOrUpdateTmpFile(TmpFile file)
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

            request.Items.Add(
                new ReplaceableItem
                {
                    Name = $"{IdPrefix}:{file.Id}",
                    Attributes = attributes
                }
            );

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

            await SaveFileContent(file);

            await PurgeTmpFiles();
        }

        public async Task SaveFileContent(TmpFile file)
        {
            await base.saveBase64AsFile(file.Content, file.Id);
        }

        /// <summary>
        /// Loads all TmpFiles - optionally loading the file content
        /// </summary>
        /// <param name="loadFile"></param>
        /// <returns></returns>
        public async Task<List<TmpFile>> GetTmpFiles(bool loadFile = true)
        {
            var files = new List<TmpFile>();
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
                    files.Add(new TmpFile
                    {
                        Id = item.Attributes.FirstOrDefault(a => a.Name == "Id")?.Value,
                        Created = DateTime.Parse(item.Attributes.FirstOrDefault(a => a.Name == "Created")?.Value ?? DateTime.MinValue.ToString()),
                        Content = null
                    });
                }
            }

            return files;
        }

        public async Task<TmpFile> GetTmpFile(string id)
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
                return new TmpFile
                {
                    Id = items.First().Attributes.FirstOrDefault(a => a.Name == "Id")?.Value,
                    Created = DateTime.Parse(items.First().Attributes.FirstOrDefault(a => a.Name == "Created")?.Value ?? DateTime.MinValue.ToString()),
                    Content = null
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
                    await base.deleteFile(id);
                }
                await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
        }

        private async Task<TmpFile> GetTmpFileFromDbItem(Item item, bool loadFile)
        {
            var file = new TmpFile();
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
                file.Content = await base.getFileAsBase64(file.Id);
            }

            return file;
        }

        public async Task PurgeTmpFiles()
        {
            var tmpFiles = await GetTmpFiles();

            foreach (var file in tmpFiles)
            {
//                if (file.Created < DateTime.Now.AddDays(-DAYS_TO_EXPIRE))
                if (file.Created < DateTime.Now.AddMinutes(-1))
                {
                        _logger.LogInformation($"Purging TmpFile: {file.Id}, Created: {file.Created}");
                    await DeleteTmpFile(file.Id, file.Content != null);
                }
            }
        }

    }
}