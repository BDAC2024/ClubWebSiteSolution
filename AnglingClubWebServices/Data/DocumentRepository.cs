using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Helpers;
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
        private const string _idPrefix = "Document";

        private readonly ILogger<DocumentRepository> _logger;
        private readonly RepositoryOptions _options;

        public DocumentRepository(
            IOptions<RepositoryOptions> opts,
            ILoggerFactory loggerFactory) : base(opts.Value, loggerFactory)
        {
            _options = opts.Value;
            _logger = loggerFactory.CreateLogger<DocumentRepository>();
        }

        public async Task AddOrUpdateAndIndexDocument(DocumentMeta file)
        {
            var wordStream = await base.getFile(file.StoredFileName, _options.DocumentBucket);

            var docText = WordTextExtractor.ExtractAndNormalizeText(wordStream, Syncfusion.DocIO.FormatType.Automatic);

            var compressedText = TextCompression.GzipCompressUtf8(docText);

            file.Searchable = true;

            await AddOrUpdateDocument(file);
            var searchDataFileName = file.StorageSearchFilename();

            await base.saveFile(searchDataFileName, compressedText, "application/text", _options.DocumentBucket);
        }

        public async Task AddOrUpdateDocument(DocumentMeta file)
        {
            using (var client = GetClient())
            {
                if (file.IsNewItem)
                {
                    file.DbKey = file.GenerateDbKey(_idPrefix);
                }

                // Store the Id as a main attribute
                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "UploadedBy", Value = file.UploadedByMembershipNumber.ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "StoredFileName", Value = file.StoredFileName, Replace = true },
                    new ReplaceableAttribute { Name = "OriginalFileName", Value = file.OriginalFileName, Replace = true },
                    new ReplaceableAttribute { Name = "Created", Value = dateToStorageString(file.Created), Replace = true },
                    new ReplaceableAttribute { Name = "Title", Value = file.Title, Replace = true },
                    new ReplaceableAttribute { Name = "Notes", Value = file.Notes, Replace = true },
                    new ReplaceableAttribute { Name = "DocumentType", Value = ((int)file.DocumentType).ToString(), Replace = true },
                    new ReplaceableAttribute { Name = "Searchable", Value = file.Searchable ? "1" : "0", Replace = true },
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

        public async Task<DocumentMeta> GetById(string docId)
        {
            var items = await GetData(_idPrefix, $"AND ItemName() = '{docId}'");

            return processItems(items).FirstOrDefault();
        }

        public async Task<List<DocumentMeta>> Get()
        {
            var files = new List<DocumentMeta>();
            var items = await GetData(_idPrefix);

            foreach (var item in processItems(items))
            {
                files.Add(item);
            }
            return files;
        }

        private List<DocumentMeta> processItems(List<Item> items)
        {
            var files = new List<DocumentMeta>();

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
                            doc.Created = dateFromStorageString(attribute.Value);
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

                        case "Searchable":
                            doc.Searchable = attribute.Value == "0" ? false : true;
                            break;

                        default:
                            break;
                    }
                }

                files.Add(doc);
            }
            return files;
        }

        public async Task<string> GetRawText(DocumentMeta doc)
        {

            var compressedContent = await base.getFile(doc.StorageSearchFilename(), _options.DocumentBucket);

            var uncompressedText = TextCompression.GzipDecompressUtf8(compressedContent.GetBuffer());

            return uncompressedText;
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

            var doc = (await Get()).SingleOrDefault(x => x.DbKey == id);
            if (doc == null)
            {
                throw new NotFoundException($"Document '{id}' was not found.");
            }

            DeleteAttributesRequest request = new DeleteAttributesRequest
            {
                DomainName = Domain,
                ItemName = $"{id}"
            };

            try
            {
                if (doc.Searchable)
                {
                    var searchDataFile = doc.StorageSearchFilename();
                    try
                    {
                        await base.deleteFile(searchDataFile, _options.DocumentBucket);
                    }
                    catch (Exception)
                    {
                        //Fail silently if unable to delete search data
                        _logger.LogWarning($"Failed to delete search data {searchDataFile}");
                    }
                }
                try
                {
                    await base.deleteFile(doc.StoredFileName, _options.DocumentBucket);
                }
                catch (Exception)
                {
                    //Fail silently if unable to delete doc file
                    _logger.LogWarning($"Failed to delete document file {doc.StoredFileName}");
                }
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