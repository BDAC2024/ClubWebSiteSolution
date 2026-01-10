using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.Models;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Inputs;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

namespace AnglingClubWebsite.Services
{
    public class DocumentService : DataServiceBase, IDocumentService
    {
        private static string CONTROLLER = "Document";

        private readonly ILogger<DocumentService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public DocumentService(
            IHttpClientFactory httpClientFactory,
            ILogger<DocumentService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }

        public async Task<FileUploadUrlResult?> GetDocumentUploadUrl(UploadFiles file, DocumentType docType)
        {
            var resp = new FileUploadUrlResult();

            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_DOCUMENT_GETUPLOADURL}";

            _logger.LogInformation($"getDocumentUploadUrl: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var model = new FileUploadUrlDto
            {
                Path = docType.StoragePath(),
                Filename = file.FileInfo.Name,
                ContentType = file.File.ContentType
            };

            var response = await Http.PostAsync($"{relativeEndpoint}", JsonContent.Create(model));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"getDocumentUploadUrl: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<FileUploadUrlResult>();
                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"getDocumentUploadUrl: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task UploadDocumentWithPresignedUrl(string uploadUrl, UploadFiles selectedFile)
        {
            // IMPORTANT: Syncfusion provides a stream
            await using var fileStream = selectedFile.File.OpenReadStream();

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
                throw new InvalidOperationException($"UploadDocumentWithPresignedUrl: upload failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
            }

        }

    }

}
