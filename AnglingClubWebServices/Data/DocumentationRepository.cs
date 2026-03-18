using Amazon.S3;
using Amazon.S3.Model;
using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnglingClubWebServices.Data
{
    public class DocumentationRepository : RepositoryBase, IDocumentationRepository
    {
        private const string ExcludedRootFolder = "Meetings/Minutes";

        private readonly RepositoryOptions _options;

        public DocumentationRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _options = opts.Value;
        }

        public async Task<List<DocumentationBucketItemDto>> GetDocumentationItems()
        {
            var results = new List<DocumentationBucketItemDto>();

            using var s3Client = GetS3Client();
            string? continuationToken = null;

            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _options.DocumentBucket,
                    ContinuationToken = continuationToken,
                    MaxKeys = 1000
                };

                var listResponse = await s3Client.ListObjectsV2Async(listRequest);

                foreach (var s3Object in listResponse.S3Objects)
                {
                    if (isExcluded(s3Object.Key))
                    {
                        continue;
                    }

                    results.Add(new DocumentationBucketItemDto
                    {
                        Key = s3Object.Key,
                        LastModifiedUtc = s3Object.LastModified.ToUniversalTime(),
                        IsFolderPlaceholder = s3Object.Key.EndsWith('/')
                    });
                }

                continuationToken = listResponse.IsTruncated ? listResponse.NextContinuationToken : null;
            }
            while (!string.IsNullOrWhiteSpace(continuationToken));

            return results;
        }

        public async Task CreateFolder(string folderPath)
        {
            var normalized = normalizeFolderPath(folderPath);

            using var s3Client = GetS3Client();
            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _options.DocumentBucket,
                Key = normalized,
                ContentBody = string.Empty,
                ContentType = "application/x-directory"
            });
        }

        public async Task<DocumentationUploadUrlResponse> GetUploadUrl(DocumentationUploadUrlRequest req)
        {
            var folderPath = normalizeFolderPath(req.FolderPath);
            var filename = Path.GetFileName(req.FileName.Trim());
            var key = $"{folderPath}{filename}";

            if (isExcluded(key))
            {
                throw new AppValidationException("Uploads into the Meetings/Minutes path are not permitted from this page.");
            }

            if (!req.OverwriteExisting && await objectExists(key))
            {
                return new DocumentationUploadUrlResponse
                {
                    FileAlreadyExists = true,
                    StorageKey = key
                };
            }

            var uploadUrl = await getPreSignedUploadUrl(key, req.ContentType, _options.DocumentBucket);

            return new DocumentationUploadUrlResponse
            {
                FileAlreadyExists = false,
                StorageKey = key,
                UploadUrl = uploadUrl
            };
        }

        public async Task<string> GetDownloadUrl(string key, string returnedFilename, int minutesBeforeExpiry = 10)
        {
            if (isExcluded(key))
            {
                throw new AppValidationException("Downloads from the Meetings/Minutes path are not permitted from this page.");
            }

            if (!await objectExists(key))
            {
                throw new AppNotFoundException("Requested file does not exist.");
            }

            return getFilePresignedUrl(key, _options.DocumentBucket, minutesBeforeExpiry, DownloadType.attachment, returnedFilename);
        }

        private async Task<bool> objectExists(string key)
        {
            using var s3Client = GetS3Client();
            try
            {
                await s3Client.GetObjectMetadataAsync(_options.DocumentBucket, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        private static bool isExcluded(string key)
        {
            return key.Equals(ExcludedRootFolder, StringComparison.OrdinalIgnoreCase)
                || key.StartsWith($"{ExcludedRootFolder}/", StringComparison.OrdinalIgnoreCase);
        }

        private static string normalizeFolderPath(string folderPath)
        {
            var normalized = (folderPath ?? string.Empty)
                .Trim()
                .Trim('/')
                .Replace("\\", "/", StringComparison.Ordinal);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (isExcluded(normalized) || normalized.StartsWith(ExcludedRootFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new AppValidationException("The Meetings/Minutes path is managed elsewhere and cannot be changed here.");
            }

            return normalized.EndsWith('/') ? normalized : $"{normalized}/";
        }
    }
}
