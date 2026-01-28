using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.Models;
using AnglingClubWebServices.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class DocumentController : AnglingClubControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IDocumentService _documentService;
        private readonly ITmpFileRepository _tmpFileRepository;
        private readonly IMapper _mapper;

        public DocumentController(
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IDocumentRepository documentRepository,
            IMemberRepository memberRepository,
            IDocumentService documentService,
            ITmpFileRepository tmpFileRepository)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DocumentController>();
            _documentRepository = documentRepository;
            base.Logger = _logger;
            _memberRepository = memberRepository;
            _documentService = documentService;
            _tmpFileRepository = tmpFileRepository;
        }

        [HttpPost("GetDocuments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentListItem>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetDocuments([FromBody] DocumentSearchRequest req)
        {
            StartTimer();

            var items = await _documentService.GetDocuments(req);

            ReportTimer("Getting document items");

            return Ok(items);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<DocumentMetaDTO> docItems)
        {
            StartTimer();

            try
            {
                foreach (var docItem in docItems)
                {
                    docItem.Created = docItem.CreatedOffset.DateTime;
                    await _documentService.SaveDocument(docItem, CurrentUser.MembershipNumber);
                }
                ReportTimer("Posting document item");
                return Ok();
            }
            catch (Exception ex)
            {
                var errMsg = "Failed to save document";
                _logger.LogError(errMsg, ex);
                return BadRequest(errMsg);
            }
        }

        /// <summary>
        /// Initial attempts to upload the file as an arg to a web api call failed on AWS with a 413 (content too large) error.
        /// The approach here is to get a pre-signed URL from the web api, then use that URL to upload the file directly to S3.
        /// The solution was obtained from ChatGPT
        /// </summary>
        /// <param name="docUploadUrlDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("GetUploadUrl")]
        public async Task<IActionResult> GetUploadUrl([FromBody] FileUploadUrlDto docUploadUrlDto)
        {
            var seperator = docUploadUrlDto.Path.IsNullOrEmpty() ? "" : (docUploadUrlDto.Path.EndsWith("/") ? "" : "/");
            var fileId = $"{docUploadUrlDto.Path}{seperator}{Guid.NewGuid().ToString()}";

            var url = await _documentRepository.GetDocumentUploadUrl(fileId, docUploadUrlDto.ContentType);

            return Ok(new FileUploadUrlResult { UploadUrl = url, UploadedFileName = fileId });
        }

        [HttpGet("minutes/readonly/{id}")]
        public async Task<IActionResult> GetReadOnlyMinutes(string id)
        {
            var doc = (await _documentRepository.Get()).SingleOrDefault(x => x.DbKey == id);

            if (doc == null)
            {
                return BadRequest("Document could not be found.");
            }

            var effectiveSeason = EnumUtils.SeasonForDate(doc.Created).Value;
            var member = (await _memberRepository.GetMembers(effectiveSeason)).FirstOrDefault(x => x.MembershipNumber == CurrentUser.MembershipNumber);

            var name = member != null ? member.Name : "";
            var fileName = doc.StoredFileName;
            var requestedAt = DateTime.Now;

            var pdfBytes = await _documentService.GenerateWatermarkedPdfFromWordDocument(
                fileName: fileName,
                watermarkText: $"COPY FOR {name.ToUpper()}",
                footerText: $"Requested by {name} on {requestedAt.ToString("dd MMM yyyy")} at {requestedAt.ToString("hh:mm tt")}",
                ct: HttpContext.RequestAborted);

            var pdfFileName = Path.ChangeExtension(fileName, ".pdf");

            await _tmpFileRepository.SaveTmpFile(pdfFileName, pdfBytes, "application/pdf");

            var url = await _tmpFileRepository.GetFilePresignedUrl(pdfFileName, SharedConstants.MINUTES_TO_EXPIRE_LINKS, "application/pdf");

            return Ok(url);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var doc = (await _documentRepository.Get()).SingleOrDefault(x => x.DbKey == id);

            if (doc == null)
            {
                return BadRequest("Document could not be found.");
            }

            if (doc.DocumentType == DocumentType.MeetingMinutes && !CurrentUser.Secretary)
            {
                return Unauthorized("Only club secretaries can download meeting minutes.");
            }

            var url = await _documentRepository.GetFilePresignedUrl(doc.StoredFileName, doc.OriginalFileName, SharedConstants.MINUTES_TO_EXPIRE_LINKS);

            return Ok(url);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> Delete(string id)
        {
            StartTimer();

            try
            {
                await _documentRepository.DeleteDocument(id);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                ReportTimer("Deleting document");
            }

        }

    }
}
