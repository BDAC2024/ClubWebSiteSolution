using Amazon.S3;
using Amazon.S3.Model;
using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class DocumentationRepository : RepositoryBase, IDocumentationRepository
    {
        private const string ExcludedRootFolder = "Meetings/Minutes";
        private const string BackupFolderName = "_backup";
        private const int MaxBackupsPerFile = 5;

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
                        LastModifiedUtc = s3Object.LastModified.Value.ToUniversalTime(),
                        IsFolderPlaceholder = s3Object.Key.EndsWith('/')
                    });
                }

                continuationToken = listResponse.IsTruncated.Value ? listResponse.NextContinuationToken : null;
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

        public async Task DeleteFile(string key)
        {
            var normalizedKey = normalizeFileKey(key);

            if (!await objectExists(normalizedKey))
            {
                throw new AppNotFoundException("Requested file does not exist.");
            }

            using var s3Client = GetS3Client();
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.DocumentBucket,
                Key = normalizedKey
            });
        }

        public async Task CreateBackup(string key)
        {
            var normalizedKey = normalizeFileKey(key);

            if (isBackupPath(normalizedKey))
            {
                throw new AppValidationException("Backup files cannot be backed up again.");
            }

            if (!await objectExists(normalizedKey))
            {
                throw new AppNotFoundException("Requested file does not exist.");
            }

            var sourceDirectory = Path.GetDirectoryName(normalizedKey)?.Replace("\\", "/", StringComparison.Ordinal) ?? string.Empty;
            var sourceFileName = Path.GetFileName(normalizedKey);
            var backupPrefix = string.IsNullOrWhiteSpace(sourceDirectory)
                ? $"{BackupFolderName}/"
                : $"{sourceDirectory}/{BackupFolderName}/";
            var backupKey = $"{backupPrefix}{addTimestampSuffix(sourceFileName, DateTime.UtcNow)}";

            using var s3Client = GetS3Client();

            // Keep explicit folder placeholders for consistency with the tree view.
            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _options.DocumentBucket,
                Key = backupPrefix,
                ContentBody = string.Empty,
                ContentType = "application/x-directory"
            });

            await s3Client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _options.DocumentBucket,
                SourceKey = normalizedKey,
                DestinationBucket = _options.DocumentBucket,
                DestinationKey = backupKey
            });

            await trimBackups(s3Client, backupPrefix, sourceFileName, MaxBackupsPerFile);
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

            if (isBackupPath(folderPath) || isBackupPath(key))
            {
                throw new AppValidationException("Uploads into _backup folders are not permitted.");
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

        private static bool isBackupPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var normalized = path.Replace("\\", "/", StringComparison.Ordinal).Trim('/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            return normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Any(x => string.Equals(x, BackupFolderName, StringComparison.OrdinalIgnoreCase));
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

            if (isBackupPath(normalized))
            {
                throw new AppValidationException("Backup are managed automatically. You cannot upload here.");
            }

            return normalized.EndsWith('/') ? normalized : $"{normalized}/";
        }

        private static string normalizeFileKey(string key)
        {
            var normalized = (key ?? string.Empty)
                .Trim()
                .Trim('/')
                .Replace("\\", "/", StringComparison.Ordinal);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new AppValidationException("A file key is required.");
            }

            if (normalized.EndsWith('/'))
            {
                throw new AppValidationException("Only files can be deleted.");
            }

            if (isExcluded(normalized))
            {
                throw new AppValidationException("The Meetings/Minutes path is managed elsewhere and cannot be changed here.");
            }

            return normalized;
        }

        private static string addTimestampSuffix(string fileName, DateTime timestampUtc)
        {
            var ext = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var suffix = timestampUtc.ToString("yyyyMMddHHmmss");

            return string.IsNullOrWhiteSpace(ext)
                ? $"{nameWithoutExt}_{suffix}"
                : $"{nameWithoutExt}_{suffix}{ext}";
        }

        private async Task trimBackups(IAmazonS3 s3Client, string backupPrefix, string sourceFileName, int maxToKeep)
        {
            var ext = Path.GetExtension(sourceFileName);
            var baseName = Path.GetFileNameWithoutExtension(sourceFileName);
            var backupPattern = new Regex(
                $"^{Regex.Escape(baseName)}_\\d{{14}}{Regex.Escape(ext)}$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var matchingBackups = new List<S3Object>();
            string? continuationToken = null;

            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _options.DocumentBucket,
                    Prefix = backupPrefix,
                    ContinuationToken = continuationToken,
                    MaxKeys = 1000
                };

                var listResponse = await s3Client.ListObjectsV2Async(listRequest);
                matchingBackups.AddRange(
                    listResponse.S3Objects
                        .Where(x => !x.Key.EndsWith('/'))
                        .Where(x =>
                        {
                            var backupFileName = Path.GetFileName(x.Key);
                            return backupPattern.IsMatch(backupFileName);
                        }));

                continuationToken = listResponse.IsTruncated.Value ? listResponse.NextContinuationToken : null;
            }
            while (!string.IsNullOrWhiteSpace(continuationToken));

            var deleteCandidates = matchingBackups
                .OrderByDescending(x => x.LastModified)
                .ThenByDescending(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Skip(maxToKeep)
                .ToList();

            foreach (var candidate in deleteCandidates)
            {
                await s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _options.DocumentBucket,
                    Key = candidate.Key
                });
            }
        }
    }
}
