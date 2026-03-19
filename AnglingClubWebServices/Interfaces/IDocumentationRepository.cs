using AnglingClubShared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentationRepository
    {
        Task<List<DocumentationBucketItemDto>> GetDocumentationItems();
        Task CreateFolder(string folderPath);
        Task DeleteFile(string key);
        Task<DocumentationUploadUrlResponse> GetUploadUrl(DocumentationUploadUrlRequest req);
        Task<string> GetDownloadUrl(string key, string returnedFilename, int minutesBeforeExpiry = 10);
    }
}
