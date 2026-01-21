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
    }
}