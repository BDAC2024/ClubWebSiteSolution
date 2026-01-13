using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Data
{
    public class DocumentRepository : RepositoryBase, IDocumentRepository
    {
        private const string IdPrefix = "Document";
        private readonly ILogger<DocumentRepository> _logger;
        private readonly RepositoryOptions _options;

        public DocumentRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _options = opts.Value;
            _logger = loggerFactory.CreateLogger<DocumentRepository>();
        }

        public async Task AddOrUpdateDocument(DocumentMeta file)
        {
            using (var client = GetClient())
            {
                if (file.IsNewItem)
                {
                    file.DbKey = file.GenerateDbKey(IdPrefix);
                }

                // Store the Id as a main attribute
                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "UploadedBy", Value = file.UploadedByMembershipNumber.ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "StoredFileName", Value = file.StoredFileName, Replace = true },
                    new ReplaceableAttribute { Name = "OriginalFileName", Value = file.OriginalFileName, Replace = true },
                    new ReplaceableAttribute { Name = "Created", Value = dateToString(file.Created), Replace = true },
                    new ReplaceableAttribute { Name = "Title", Value = file.Title, Replace = true },
                    new ReplaceableAttribute { Name = "Notes", Value = file.Notes, Replace = true },
                    new ReplaceableAttribute { Name = "DocumentType", Value = ((int)file.DocumentType).ToString(), Replace = true }
                };

                base.SetupTableAttribues(request, file.DbKey, attributes);

                try
                {
                    await WriteInBatches(request, client);
                    _logger.LogDebug($"Document meta added: {file.DbKey} - {file.Title}");
                }
                catch (AmazonSimpleDBException ex)
                {
                    _logger.LogError(ex, $"Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                    throw;
                }
            }
        }

        public async Task<List<DocumentMeta>> Get()
        {
            var files = new List<DocumentMeta>();
            var items = await GetData(IdPrefix);

            foreach (var item in items)
            {
                var doc = new DocumentMeta();

                doc.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "UploadedBy":
                            doc.UploadedByMembershipNumber = Convert.ToInt32(attribute.Value);
                            break;

                        case "StoredFileName":
                            doc.StoredFileName = attribute.Value;
                            break;

                        case "OriginalFileName":
                            doc.OriginalFileName = attribute.Value;
                            break;

                        case "Created":
                            doc.Created = DateTime.Parse(attribute.Value);
                            break;

                        case "DocumentType":
                            doc.DocumentType = (DocumentType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Title":
                            doc.Title = attribute.Value;
                            break;

                        case "Notes":
                            doc.Notes = attribute.Value;
                            break;

                        default:
                            break;
                    }
                }

                files.Add(doc);
            }
            return files;
        }

        public async Task<WordDocument> GetWordDocument(string fileName)
        {
            var wordStream = await base.getFile(fileName, _options.DocumentBucket);

            // 2) Load Word doc
            var wordDoc = new WordDocument(wordStream, Syncfusion.DocIO.FormatType.Automatic);
            DocioFontSubstitution.AttachLatoSubstitution(wordDoc);

            return wordDoc;
        }

        /// <summary>
        /// Initial attempts to upload the file as an arg to a web api call failed on AWS with a 413 (content too large) error.
        /// The approach here is to get a pre-signed URL from the web api, then use that URL to upload the file directly to S3.
        /// The solution was obtained from ChatGPT
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<string> GetDocumentUploadUrl(string filename, string contentType)
        {
            return await base.getPreSignedUploadUrl(filename, contentType, _options.DocumentBucket);
        }

        public async Task DeleteDocument(string id)
        {
            var client = GetClient();

            var doc = (await Get()).Single(x => x.DbKey == id);

            DeleteAttributesRequest request = new DeleteAttributesRequest
            {
                DomainName = Domain,
                ItemName = $"{id}"
            };

            try
            {
                await base.deleteFile(doc.StoredFileName, _options.DocumentBucket);
                await client.DeleteAttributesAsync(request);
            }
            catch (AmazonSimpleDBException ex)
            {
                _logger.LogError(ex, $"DeleteDocument Error Code: {ex.ErrorCode}, Error Type: {ex.ErrorType}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteDocument failed");
                throw;
            }
        }

        public async Task<string> GetFilePresignedUrl(string storedFileName, string returnedFileName, int minutesBeforeExpiry)
        {
            await Task.Delay(0);

            return base.getFilePresignedUrl(storedFileName, _options.DocumentBucket, minutesBeforeExpiry, DownloadType.attachment, returnedFileName);
        }


    }
}