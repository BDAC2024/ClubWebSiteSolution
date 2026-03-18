using AnglingClubShared.DTOs;
using AnglingClubShared.Extensions;
using AnglingClubWebServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    public class DocumentationController : AnglingClubControllerBase
    {
        private readonly IDocumentationRepository _documentationRepository;
        private const string ExcludedRoot = "Meetings/Minutes";

        public DocumentationController(IDocumentationRepository documentationRepository)
        {
            _documentationRepository = documentationRepository;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetFolderTree()
        {
            var files = await _documentationRepository.GetAllFiles();
            var folderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var key = Normalize(file.Id);

                if (IsExcluded(key))
                {
                    continue;
                }

                if (key.EndsWith("/"))
                {
                    AddFolderAndParents(folderSet, key.TrimEnd('/'));
                    continue;
                }

                var folderPath = ParentPath(key);
                if (!folderPath.IsNullOrEmpty())
                {
                    AddFolderAndParents(folderSet, folderPath);
                }
            }

            return Ok(new DocumentationFolderTreeDto
            {
                FolderPaths = folderSet.OrderBy(x => x).ToList()
            });
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetFiles([FromQuery] string folderPath = "")
        {
            var normalizedFolderPath = Normalize(folderPath).Trim('/');
            var files = await _documentationRepository.GetAllFiles();

            var rows = files
                .Where(x =>
                {
                    var key = Normalize(x.Id);

                    if (key.EndsWith("/") || IsExcluded(key))
                    {
                        return false;
                    }

                    return string.Equals(ParentPath(key), normalizedFolderPath, StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var key = Normalize(x.Id);
                    return new DocumentationFileItemDto
                    {
                        Key = key,
                        FileName = FileNameFromPath(key),
                        Created = x.Created,
                        SizeBytes = 0
                    };
                })
                .ToList();

            return Ok(rows);
        }

        [HttpPost("create-folder")]
        public async Task<IActionResult> CreateFolder([FromBody] DocumentationCreateFolderRequestDto req)
        {
            var parentFolder = Normalize(req.ParentFolderPath).Trim('/');
            var folderName = Normalize(req.FolderName).Trim('/');

            if (folderName.IsNullOrEmpty())
            {
                return BadRequest("Folder name is required.");
            }

            var fullPath = parentFolder.IsNullOrEmpty() ? folderName : $"{parentFolder}/{folderName}";

            if (IsExcluded(fullPath))
            {
                return BadRequest("Cannot create folders in a protected path.");
            }

            await _documentationRepository.CreateFolder(fullPath);
            return Ok();
        }

        [HttpPost("upload-url")]
        public async Task<IActionResult> GetUploadUrl([FromBody] DocumentationUploadUrlRequestDto req)
        {
            var normalizedFolderPath = Normalize(req.FolderPath).Trim('/');
            var fileName = FileNameFromPath(Normalize(req.FileName));

            if (fileName.IsNullOrEmpty())
            {
                return BadRequest("File name is required.");
            }

            var key = normalizedFolderPath.IsNullOrEmpty() ? fileName : $"{normalizedFolderPath}/{fileName}";

            if (IsExcluded(key))
            {
                return BadRequest("Cannot upload to a protected path.");
            }

            var exists = await _documentationRepository.FileExists(key);

            if (exists && !req.Overwrite)
            {
                return Ok(new DocumentationUploadUrlResultDto
                {
                    AlreadyExists = true,
                    RequiresOverwriteConfirmation = true,
                    UploadedFileName = key
                });
            }

            var uploadUrl = await _documentationRepository.GetUploadUrl(key, req.ContentType);

            return Ok(new DocumentationUploadUrlResultDto
            {
                AlreadyExists = exists,
                RequiresOverwriteConfirmation = false,
                UploadUrl = uploadUrl,
                UploadedFileName = key
            });
        }

        [HttpGet("download")]
        public IActionResult GetDownloadUrl([FromQuery] string key)
        {
            var normalizedKey = Normalize(key);

            if (normalizedKey.IsNullOrEmpty() || IsExcluded(normalizedKey))
            {
                return BadRequest("Invalid file key.");
            }

            var url = _documentationRepository.GetDownloadUrl(normalizedKey, FileNameFromPath(normalizedKey));
            return Ok(url);
        }

        private static string Normalize(string value)
        {
            return value?.Trim().Replace("\\", "/") ?? "";
        }

        private static string ParentPath(string key)
        {
            if (key.IsNullOrEmpty())
            {
                return "";
            }

            var idx = key.LastIndexOf('/');
            return idx <= 0 ? "" : key.Substring(0, idx);
        }

        private static string FileNameFromPath(string key)
        {
            if (key.IsNullOrEmpty())
            {
                return "";
            }

            var idx = key.LastIndexOf('/');
            return idx < 0 ? key : key.Substring(idx + 1);
        }

        private static void AddFolderAndParents(HashSet<string> folderSet, string folderPath)
        {
            var parts = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = "";

            foreach (var part in parts)
            {
                current = current.IsNullOrEmpty() ? part : $"{current}/{part}";
                folderSet.Add(current);
            }
        }

        private static bool IsExcluded(string key)
        {
            var normalized = Normalize(key).Trim('/');

            return normalized.Equals(ExcludedRoot, StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith($"{ExcludedRoot}/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
