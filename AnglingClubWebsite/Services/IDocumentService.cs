using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentListItem>?> ReadDocuments(DocumentSearchRequest req);
        Task SaveDocument(DocumentMeta item);

        Task<FileUploadUrlResult?> GetDocumentUploadUrl(UploadFiles file, DocumentMeta docType);
        Task UploadDocumentWithPresignedUrl(string uploadUrl, UploadFiles selectedFile);
        Task<string?> GetReadOnlyUrl(string id);
        Task DeleteDocument(string id);
        Task<string?> Download(string id);

        Task<DocumentationListingDto?> GetDocumentationListing(string folderPath);
        Task<DocumentationUploadUrlResultDto?> GetDocumentationUploadUrl(string folderPath, UploadFiles file, bool overwriteIfExists);
        Task CreateDocumentationFolder(string parentPath, string folderName);
        Task<string?> GetDocumentationDownloadUrl(string fileKey);
    }
}