using Amazon.S3;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class DocumentationRepository : RepositoryBase, IDocumentationRepository
    {
        private readonly RepositoryOptions _options;

        public DocumentationRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _options = opts.Value;
        }

        public async Task<List<StoredFileMeta>> GetAllFiles()
        {
            return await getFilesFromS3(_options.DocumentBucket);
        }

        public async Task<bool> FileExists(string key)
        {
            using var s3Client = GetS3Client();

            try
            {
                await s3Client.GetObjectMetadataAsync(_options.DocumentBucket, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<string> GetUploadUrl(string key, string contentType)
        {
            return await getPreSignedUploadUrl(key, contentType, _options.DocumentBucket);
        }

        public string GetDownloadUrl(string key, string returnedFileName)
        {
            return getFilePresignedUrl(key, _options.DocumentBucket, 10, DownloadType.attachment, returnedFileName);
        }

        public async Task CreateFolder(string folderPath)
        {
            var normalizedFolderPath = folderPath?.Trim().Trim('/').Replace("\\", "/") ?? "";
            if (string.IsNullOrWhiteSpace(normalizedFolderPath))
            {
                return;
            }

            await saveFile($"{normalizedFolderPath}/", Array.Empty<byte>(), "application/x-directory", _options.DocumentBucket, false);
        }
    }
}
