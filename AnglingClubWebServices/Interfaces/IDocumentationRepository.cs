using AnglingClubShared.DTOs;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentationRepository
    {
        Task<List<DocumentationBucketItemDto>> GetDocumentationItems();
        Task CreateFolder(string folderPath);
        Task<DocumentationUploadUrlResponse> GetUploadUrl(DocumentationUploadUrlRequest req);
        Task<string> GetDownloadUrl(string key, string returnedFilename, int minutesBeforeExpiry = 10);
    }
}
