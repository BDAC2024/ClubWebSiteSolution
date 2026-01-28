using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Blazor.Inputs;
using System.Net.Http.Json;
using System.Text.Json;

namespace AnglingClubWebsite.Services
{
    public class BookPrintingService : DataServiceBase, IBookPrintingService
    {
        private static string _controller = "BookPrinting";

        private readonly ILogger<BookPrintingService> _logger;
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ITmpFileService _tmpFileService;

        public BookPrintingService(
            IHttpClientFactory httpClientFactory,
            ILogger<BookPrintingService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            ITmpFileService tmpFileService) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _tmpFileService = tmpFileService;
        }

        /// <summary>
        /// Do not include the path, just the filename
        /// </summary>
        /// <param name="filename">Do not include the path, just the filename</param>
        /// <returns></returns>
        public async Task<BookPrintingResult?> GetPrintReadyPDFs(UploadFiles? file)
        {
            var uploadUrlDetails = await _tmpFileService.GetFileUploadUrl(file!, TmpFileType.BookPrinting.UploadPath());

            if (uploadUrlDetails == null)
            {
                throw new Exception("There was an error uploading the PDF file.");
            }
            await _tmpFileService.UploadFileWithPresignedUrl(uploadUrlDetails.UploadUrl, file!);

            var relativeEndpoint = $"{_controller}";

            _logger.LogInformation($"GetPrintReadyPDFs: Accessing {Http.BaseAddress}{relativeEndpoint}");

            var response = await Http.GetAsync($"{relativeEndpoint}/{Path.GetFileName(uploadUrlDetails.UploadedFileName)}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"GetPrintReadyPDFs: failed to return success: error {response.StatusCode} - {response.ReasonPhrase}");

                var body = await response.Content.ReadAsStringAsync();
                var pd = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (pd != null)
                {
                    _messenger.Send(new ShowMessage(MessageState.Error, "Print-ready generation failed", pd.Detail));
                }
                return null;
            }
            else
            {
                try
                {
                    var content = await response.Content.ReadFromJsonAsync<BookPrintingResult>();

                    return content;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetPrintReadyPDFs: {ex.Message}");
                    throw;
                }
            }
        }

    }

}
