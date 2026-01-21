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
    public class TmpFileController : ControllerBase
    {
        private readonly ILogger<TmpFileController> _logger;
        private readonly ITmpFileRepository _tmpFileRepository;
        private readonly IMapper _mapper;

        public TmpFileController(
            IMapper mapper,
            ILoggerFactory loggerFactory,
            ITmpFileRepository tmpFileRepository)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<TmpFileController>();
            _tmpFileRepository = tmpFileRepository;
        }

        /// <summary>
        /// Initial attempts to upload the file as an arg to a web api call failed on AWS with a 413 (content too large) error.
        /// The approach here is to get a pre-signed URL from the web api, then use that URL to upload the file directly to S3.
        /// The solution was obtained from ChatGPT
        /// </summary>
        /// <param name="tmpFileUploadUrlDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("GetUploadUrl")]
        public async Task<IActionResult> GetUploadUrl([FromBody] FileUploadUrlDto tmpFileUploadUrlDto)
        {
            var seperator = tmpFileUploadUrlDto.Path.IsNullOrEmpty() ? "" : (tmpFileUploadUrlDto.Path.EndsWith("/") ? "" : "/");
            var fileId = $"{tmpFileUploadUrlDto.Path}{seperator}{Guid.NewGuid().ToString()}";

            var url = await _tmpFileRepository.GetTmpFileUploadUrl(fileId, tmpFileUploadUrlDto.ContentType);


            return Ok(new FileUploadUrlResult { UploadUrl = url, UploadedFileName = fileId });
        }

    }
}
