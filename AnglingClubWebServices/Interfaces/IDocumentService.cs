using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentService
    {
        Task<string> GetReadOnlyMinutesUrl(string id, Member user, CancellationToken ct);
        Task SaveDocument(DocumentMeta docItem, int createdByMember);
        Task<List<DocumentListItem>> GetDocuments(DocumentSearchRequest req);
        Task<string> Download(string id, Member user);
        Task<DocumentationListingDto> GetDocumentationListing(string folderPath);
        Task<DocumentationUploadUrlResultDto> GetDocumentationUploadUrl(DocumentationUploadUrlRequestDto req);
        Task CreateDocumentationFolder(CreateDocumentationFolderRequestDto req);
        Task<string> GetDocumentationDownloadUrl(string fileKey);

    }
}