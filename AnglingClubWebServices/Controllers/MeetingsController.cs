using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly ILogger<MeetingsController> _logger;
        private readonly ITmpFileRepository _tmpFileRepository;
        private readonly IDocumentService _documentService;

        public MeetingsController(
            ILoggerFactory loggerFactory, ITmpFileRepository tmpFileRepository, IDocumentService documentService)
        {
            _logger = loggerFactory.CreateLogger<MeetingsController>();
            _tmpFileRepository = tmpFileRepository;
            _documentService = documentService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Minutes()
        {
            var name = "S.Townend";
            var fileName = "Meetings/Minutes/Minutes_Example.docx";
            var requestedAt = DateTime.Now;

            var pdfBytes = await _documentService.GenerateWatermarkedPdfFromWordDocument(
                fileName: fileName,
                watermarkText: $"COPY FOR {name.ToUpper()}",
                footerText: $"Requested by {name} on {requestedAt.ToString("dd MMM yyyy")} at {requestedAt.ToString("hh:mm tt")}",
                ct: HttpContext.RequestAborted);

            var pdfFileName = Path.ChangeExtension(fileName, ".pdf");

            await _tmpFileRepository.SaveTmpFile(pdfFileName, pdfBytes, "application/pdf");

            var url = await _tmpFileRepository.GetFilePresignedUrl(pdfFileName, "application/pdf", 5);

            return Ok(url);
        }

    }
}
