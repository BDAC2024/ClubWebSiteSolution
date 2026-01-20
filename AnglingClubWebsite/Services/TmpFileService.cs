using AnglingClubShared.DTOs;
using AnglingClubShared.Models;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Inputs;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class TmpFileService : DataServiceBase, ITmpFileService
    {
        private static string _controller = "TmpFile";
        private const long _maxUploadBytes = 20 * 1024 * 1024; // 20 MB

        private readonly ILogger<TmpFileService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;

        public TmpFileService(
            IHttpClientFactory httpClientFactory,
            ILogger<TmpFileService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
        }



        public async Task<FileUploadUrlResult?> GetFileUploadUrl(UploadFiles file, string path)
        {
            var resp = new FileUploadUrlResult();

            var relativeEndpoint = $"{_controller}/{Constants.API_TMPFILE_GETUPLOADURL}";

            _logger.LogInformation($"GetFileUploadUrl: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var model = new FileUploadUrlDto
            {
                Path = path,
                Filename = file.FileInfo.Name,
                ContentType = file.File.ContentType
            };

            var response = await Http.PostAsync($"{relativeEndpoint}", JsonContent.Create(model));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"GetFileUploadUrl: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");
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
                    _logger.LogError($"GetFileUploadUrl: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task UploadFileWithPresignedUrl(string uploadUrl, UploadFiles selectedFile)
        {

            // IMPORTANT: Syncfusion provides a stream
            await using var fileStream = selectedFile.File.OpenReadStream(_maxUploadBytes);

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
                throw new InvalidOperationException($"UploadFileWithPresignedUrl: upload failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
            }

        }
    }

}
