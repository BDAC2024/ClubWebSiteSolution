using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Ocsp;
using System;
using System.Threading.Tasks;
using static NodaTime.TimeZones.ZoneEqualityComparer;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookPrintingController : AnglingClubControllerBase
    {
        private const TmpFileType _tmpFileType = TmpFileType.BookPrinting;

        #region Backing Fields

        private readonly ILogger<BookPrintingController> _logger;
        private readonly IBookPrintingService _bookPrintingService;
        private readonly ITmpFileRepository _tmpFileRepository;

        #endregion

        #region Constructors

        public BookPrintingController(
            ILoggerFactory loggerFactory,
            IAuthService authService,
            IBookPrintingService bookPrintingService,
            ITmpFileRepository tmpFileRepository)
        {
            _logger = loggerFactory.CreateLogger<BookPrintingController>();

            base.Logger = _logger;
            _bookPrintingService = bookPrintingService;
            _tmpFileRepository = tmpFileRepository;
        }

        #endregion

        #region Methods

        [HttpGet("{uploadedFilename}")]
        public async Task<IActionResult> Get(string uploadedFilename)
        {
            StartTimer();

            var filename = $"{_tmpFileType.UploadPath()}/{uploadedFilename}";
            var sourceStream = await _tmpFileRepository.GetTmpFileStream(filename);

            var options = new BookPrintingOptions
            {
                SeparateCovers = true,
                TwoBooksPerSheet = true,
                MaxPages = 200,
                RequireConsistentPageSize = true,
                MarginPoints = 0,
                GutterPoints = 0,
                OutputSheetSize = Syncfusion.Pdf.PdfPageSize.A4
            };

            byte[] covers;
            byte[] content;

            try
            {
                (covers, content) = _bookPrintingService.Impose(sourceStream, options);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Unable to impose PDF",
                    detail: ex.Message,
                    statusCode: 400);
            }

            var outputPath = _tmpFileType.OutputPath();

            var coversKey = $"{outputPath}/{uploadedFilename}_covers.pdf";
            var contentKey = $"{outputPath}/{uploadedFilename}_content.pdf";

            await _tmpFileRepository.SaveTmpFile(coversKey, covers, "application/pdf");
            await _tmpFileRepository.SaveTmpFile(contentKey, content, "application/pdf");

            // 3. Return pre-signed download URLs
            return Ok(new BookPrintingResult
            {
                CoversUrl = await _tmpFileRepository.GetFilePresignedUrl(coversKey, 60, "application/pdf"),
                ContentUrl = await _tmpFileRepository.GetFilePresignedUrl(contentKey, 60, "application/pdf"),
            });
        }



        #endregion
    }
}
