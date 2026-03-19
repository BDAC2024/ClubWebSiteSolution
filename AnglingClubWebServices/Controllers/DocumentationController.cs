using AnglingClubShared.DTOs;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
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
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            var items = await _documentationRepository.GetDocumentationItems();
            return Ok(new DocumentationListResponse { Items = items });
        }

        [HttpPost("folders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateFolder([FromBody] CreateDocumentationFolderRequest req)
        {
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            await _documentationRepository.CreateFolder(req.FolderPath);
            return Ok();
        }

        [HttpDelete("file")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteFile([FromQuery] string key)
        {
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            await _documentationRepository.DeleteFile(key);
            return Ok();
        }

        [HttpPost("upload-url")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocumentationUploadUrlResponse))]
        public async Task<IActionResult> GetUploadUrl([FromBody] DocumentationUploadUrlRequest req)
        {
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            var response = await _documentationRepository.GetUploadUrl(req);
            return Ok(response);
        }

        [HttpPost("backup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateBackup([FromBody] DocumentationBackupRequest req)
        {
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            await _documentationRepository.CreateBackup(req.Key);
            return Ok();
        }

        [HttpGet("download-url")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> GetDownloadUrl([FromQuery] string key)
        {
            if (!CurrentUser.Admin)
            {
                throw new AppForbiddenException("Only Administrators can access this.");
            }

            var fileName = getDownloadFileName(key);
            var url = await _documentationRepository.GetDownloadUrl(key, fileName, 10);
            return Ok(url);
        }

        private static string getDownloadFileName(string key)
        {
            var fileName = Path.GetFileName(key);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return fileName;
            }

            var parentFolder = Path.GetDirectoryName(key)?.Replace("\\", "/", System.StringComparison.Ordinal) ?? string.Empty;
            var inBackupFolder = parentFolder.EndsWith("/_backup", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(parentFolder, "_backup", System.StringComparison.OrdinalIgnoreCase);

            if (!inBackupFolder)
            {
                return fileName;
            }

            var ext = Path.GetExtension(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var restored = Regex.Replace(nameWithoutExt, @"_\d{14}$", string.Empty, RegexOptions.CultureInvariant);

            return string.IsNullOrWhiteSpace(ext) ? restored : $"{restored}{ext}";
        }
    }
}
