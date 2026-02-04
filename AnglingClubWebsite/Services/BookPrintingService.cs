using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Inputs;
using System.Net.Http.Json;

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

            var response = await Http.GetAsync($"{relativeEndpoint}/{Path.GetFileName(uploadUrlDetails.UploadedFileName)}");

            var content = await response.Content.ReadFromJsonAsync<BookPrintingResult>();

            return content;
        }

    }

}
