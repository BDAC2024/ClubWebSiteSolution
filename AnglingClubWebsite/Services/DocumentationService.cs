using AnglingClubShared.DTOs;
using AnglingClubWebsite.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class DocumentationService : DataServiceBase, IDocumentationService
    {
        private const string CONTROLLER = "Documentation";
        private const long MAXUPLOADBYTES = 50 * 1024 * 1024;

        public DocumentationService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<DocumentationFolderTreeDto?> GetFolderTree()
        {
            var response = await Http.GetAsync($"{CONTROLLER}/tree");
            return await response.Content.ReadFromJsonAsync<DocumentationFolderTreeDto>();
        }

        public async Task<List<DocumentationFileItemDto>?> GetFiles(string folderPath)
        {
            var response = await Http.GetAsync($"{CONTROLLER}/files?folderPath={Uri.EscapeDataString(folderPath ?? "")}");
            return await response.Content.ReadFromJsonAsync<List<DocumentationFileItemDto>>();
        }

        public async Task CreateFolder(DocumentationCreateFolderRequestDto request)
        {
            await Http.PostAsJsonAsync($"{CONTROLLER}/create-folder", request);
        }

        public async Task<DocumentationUploadUrlResultDto?> GetUploadUrl(DocumentationUploadUrlRequestDto request)
        {
            var response = await Http.PostAsJsonAsync($"{CONTROLLER}/upload-url", request);
            return await response.Content.ReadFromJsonAsync<DocumentationUploadUrlResultDto>();
        }

        public async Task UploadWithPresignedUrl(string uploadUrl, IBrowserFile file)
        {
            try
            {
                await using var fileStream = file.OpenReadStream(MAXUPLOADBYTES);
                using var content = new StreamContent(fileStream);

                content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                using var req = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = content
                };

                using var s3Http = new HttpClient();
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

        public async Task<string?> GetDownloadUrl(string key)
        {
            var response = await Http.GetAsync($"{CONTROLLER}/download?key={Uri.EscapeDataString(key)}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
