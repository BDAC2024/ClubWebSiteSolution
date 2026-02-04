using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            foreach (var docItem in docItems)
            {
                docItem.Created = docItem.CreatedOffset.DateTime;
                await _documentService.SaveDocument(docItem, CurrentUser.MembershipNumber);
            }
            ReportTimer("Posting document item");
            return Ok();
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
            var url = _documentService.GetReadOnlyMinutesUrl(id, CurrentUser, HttpContext.RequestAborted);

            return Ok(url);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var url = await _documentService.Download(id, CurrentUser);

            return Ok(url);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> Delete(string id)
        {
            StartTimer();

            await _documentRepository.DeleteDocument(id);

            return NoContent();
        }

    }
}
