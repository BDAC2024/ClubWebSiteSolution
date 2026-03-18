using AnglingClubShared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace AnglingClubWebsite.Services
{
    public interface IDocumentationService
    {
        Task<DocumentationFolderTreeDto?> GetFolderTree();
        Task<List<DocumentationFileItemDto>?> GetFiles(string folderPath);
        Task CreateFolder(DocumentationCreateFolderRequestDto request);
        Task<DocumentationUploadUrlResultDto?> GetUploadUrl(DocumentationUploadUrlRequestDto request);
        Task UploadWithPresignedUrl(string uploadUrl, IBrowserFile file);
        Task<string?> GetDownloadUrl(string key);
    }
}
