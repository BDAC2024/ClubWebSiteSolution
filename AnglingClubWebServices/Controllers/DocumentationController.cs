using AnglingClubShared.DTOs;
using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class DocumentationController : AnglingClubControllerBase
    {
        private readonly IDocumentationRepository _documentationRepository;

        public DocumentationController(IDocumentationRepository documentationRepository)
        {
            _documentationRepository = documentationRepository;
        }

        [HttpGet("items")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocumentationListResponse))]
        public async Task<IActionResult> GetItems()
        {
            var items = await _documentationRepository.GetDocumentationItems();
            return Ok(new DocumentationListResponse { Items = items });
        }

        [HttpPost("folders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateFolder([FromBody] CreateDocumentationFolderRequest req)
        {
            await _documentationRepository.CreateFolder(req.FolderPath);
            return Ok();
        }

        [HttpPost("upload-url")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocumentationUploadUrlResponse))]
        public async Task<IActionResult> GetUploadUrl([FromBody] DocumentationUploadUrlRequest req)
        {
            var response = await _documentationRepository.GetUploadUrl(req);
            return Ok(response);
        }

        [HttpGet("download-url")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> GetDownloadUrl([FromQuery] string key)
        {
            var fileName = Path.GetFileName(key);
            var url = await _documentationRepository.GetDownloadUrl(key, fileName, 10);
            return Ok(url);
        }
    }
}
