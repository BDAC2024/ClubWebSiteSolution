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

        public async Task AddOrUpdateTmpFile(DocumentMeta file)
        {
            using (var client = GetClient())
            {

                if (string.IsNullOrEmpty(file.Id))
                    throw new ArgumentException("Document Id cannot be null or empty.");

                // Store the Id as a main attribute
                BatchPutAttributesRequest request = new BatchPutAttributesRequest();
                request.DomainName = Domain;

                var attributes = new List<ReplaceableAttribute>
                {
                    new ReplaceableAttribute { Name = "Id", Value = file.Id, Replace = true },
                    new ReplaceableAttribute { Name = "Created", Value = dateToString(DateTime.Now), Replace = true },
                    new ReplaceableAttribute { Name = "Name", Value = file.OriginalFileName, Replace = true },
                    new ReplaceableAttribute { Name = "Notes", Value = file.Notes, Replace = true },
                    new ReplaceableAttribute { Name = "DocumentType", Value = file.DocumentType.ToString(), Replace = true }
                };

                base.SetupTableAttribues(request, $"{IdPrefix}:{file.Id}", attributes);

                try
                {
                    await WriteInBatches(request, client);
                    _logger.LogDebug($"Document meta added/updated: {file.Id}");
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
            var items = await GetData(IdPrefix, "AND Id > ''", "ORDER BY Id");

            foreach (var item in items)
            {
                var doc = new DocumentMeta();

                doc.DbKey = item.Name;

                foreach (var attribute in item.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "Id":
                            doc.Id = attribute.Value;
                            break;

                        case "Created":
                            doc.Created = DateTime.Parse(attribute.Value);
                            break;

                        case "DocumentType":
                            doc.DocumentType = (DocumentType)(Convert.ToInt32(attribute.Value));
                            break;

                        case "Name":
                            doc.OriginalFileName = attribute.Value;
                            break;

                        case "Notes":
                            doc.OriginalFileName = attribute.Value;
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

    }
}