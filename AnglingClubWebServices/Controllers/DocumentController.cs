using AnglingClubShared.DTOs;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;

        public DocumentController(
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IDocumentRepository documentRepository)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DocumentController>();
            _documentRepository = documentRepository;
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
