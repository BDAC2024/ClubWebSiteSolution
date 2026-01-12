using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentListItem>?> ReadDocuments(DocumentType docType);
        Task SaveDocument(DocumentMeta item);

        Task<FileUploadUrlResult?> GetDocumentUploadUrl(UploadFiles file, DocumentType docType);
        Task UploadDocumentWithPresignedUrl(string uploadUrl, UploadFiles selectedFile);
        Task<string?> GetReadOnlyUrl(string id);
    }
}