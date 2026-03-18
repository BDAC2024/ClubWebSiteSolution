using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentationRepository
    {
        Task<List<StoredFileMeta>> GetAllFiles();
        Task<bool> FileExists(string key);
        Task<string> GetUploadUrl(string key, string contentType);
        string GetDownloadUrl(string key, string returnedFileName);
        Task CreateFolder(string folderPath);
    }
}
