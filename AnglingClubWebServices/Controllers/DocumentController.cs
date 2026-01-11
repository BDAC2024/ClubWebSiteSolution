using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IMapper _mapper;

        public DocumentController(
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IDocumentRepository documentRepository,
            IMemberRepository memberRepository)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DocumentController>();
            _documentRepository = documentRepository;
            base.Logger = _logger;
            _memberRepository = memberRepository;
        }

        [HttpGet("{docType}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DocumentListItem>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> Get(DocumentType docType)
        {
            StartTimer();

            var members = await _memberRepository.GetMembers((Season?)EnumUtils.CurrentSeason());

            var dbItems = await _documentRepository.Get();

            var items = _mapper.Map<List<DocumentListItem>>(dbItems.Where(x => x.DocumentType == docType));

            foreach (var item in items)
            {
                item.UploadedBy = members.First(x => x.MembershipNumber == item.UploadedByMembershipNumber).Name;
            }

            ReportTimer("Getting document items");

            return Ok(items);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<DocumentMeta> docItems)
        {
            StartTimer();

            try
            {
                foreach (var docItem in docItems)
                {
                    docItem.UploadedByMembershipNumber = CurrentUser.MembershipNumber;
                    await _documentRepository.AddOrUpdateDocument(docItem);
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

    }
}
