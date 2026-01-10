using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Services
{
    public interface IDocumentService
    {
        Task<FileUploadUrlResult?> GetDocumentUploadUrl(UploadFiles file, DocumentType docType);
        Task UploadDocumentWithPresignedUrl(string uploadUrl, UploadFiles selectedFile);
    }
}