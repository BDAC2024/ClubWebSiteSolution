using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Inputs;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class DocumentService : DataServiceBase, IDocumentService
    {
        private const string CONTROLLER = "Document";
        private const long MAXUPLOADBYTES = 20 * 1024 * 1024; // 20 MB

        private readonly ILogger<DocumentService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;

        public DocumentService(
            IHttpClientFactory httpClientFactory,
            ILogger<DocumentService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            IMapper mapper) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }

        public async Task<List<DocumentListItem>?> ReadDocuments(DocumentSearchRequest req)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_DOCUMENT_READ}";

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", req);

            var content = await response.Content.ReadFromJsonAsync<List<DocumentListItem>>();
            return content;
        }

        public async Task SaveDocument(DocumentMeta item)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_DOCUMENT}";

            var docDTO = _mapper.Map<DocumentMetaDTO>(item);
            docDTO.CreatedOffset = docDTO.Created;

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", new List<DocumentMetaDTO> { docDTO });

            return;
        }

        /// <summary>
        /// Gets a watermarked PDF of the word document
        /// </summary>
        /// <param name="id">The dbKey of the document</param>
        /// <returns></returns>
        public async Task<string?> GetReadOnlyUrl(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_DOCUMENT}/minutes/readOnly/{id}";

            _logger.LogInformation("HttpClient.Timeout is {Timeout}", HttpLongRunning.Timeout);

            // Allow e.g. 10 minutes for conversion
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

            var response = await HttpLongRunning.GetAsync($"{relativeEndpoint}", cts.Token);

            var content = await response.Content.ReadAsStringAsync(cts.Token);
            return content;
        }

        /// <summary>
        /// Downloads the document via a presigned URL
        /// </summary>
        /// <param name="id">The dbKey of the document</param>
        /// <returns></returns>
        public async Task<string?> Download(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_DOCUMENT}/download/{id}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }


        public async Task<FileUploadUrlResult?> GetDocumentUploadUrl(UploadFiles file, DocumentMeta doc)
        {
            var resp = new FileUploadUrlResult();

            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_DOCUMENT_GETUPLOADURL}";

            var model = new FileUploadUrlDto
            {
                Path = doc.StoragePath(),
                Filename = file.FileInfo.Name,
                ContentType = file.File.ContentType
            };

            var response = await Http.PostAsync($"{relativeEndpoint}", JsonContent.Create(model));

            var content = await response.Content.ReadFromJsonAsync<FileUploadUrlResult>();
            return content;
        }

        public async Task UploadDocumentWithPresignedUrl(string uploadUrl, UploadFiles selectedFile)
        {
            try
            {
                // IMPORTANT: Syncfusion provides a stream
                await using var fileStream = selectedFile.File.OpenReadStream(MAXUPLOADBYTES);

                using var content = new StreamContent(fileStream);

                // Must match how your presigned URL was created (if Content-Type was part of the signature)
                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        selectedFile.File.ContentType
                    );

                using var req = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = content
                };

                // No auth header to S3 here; presigned URL already authorizes it.
                using var s3Http = new HttpClient(); // clean client for S3 only
                using var resp = await s3Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();

                    throw new S3UploadException(
                        userMessage: "Upload failed. Please try again.",
                        statusCode: (int)resp.StatusCode,
                        responseBody: body);
                }

            }
            catch (HttpRequestException ex)
            {
                // Common in WASM for CORS / network issues
                throw new S3UploadException(
                    userMessage: "Upload failed due to a network or browser security issue (CORS). Please try again.",
                    innerException: ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new S3UploadException(
                    userMessage: "Upload timed out. Please try again.",
                    innerException: ex);
            }
        }

        public async Task DeleteDocument(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}{Constants.API_DOCUMENT}/{id}";

            _logger.LogInformation($"DeleteDocument: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.DeleteAsync($"{relativeEndpoint}");
        }


    }

}
