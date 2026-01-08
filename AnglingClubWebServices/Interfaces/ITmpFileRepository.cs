using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITmpFileRepository
    {
        Task AddOrUpdateTmpFile(StoredFileMeta file);
        Task<List<StoredFile>> GetTmpFiles(bool loadFile = false);
        Task<StoredFile> GetTmpFile(string id);
        Task DeleteTmpFile(string id, bool deleteFromS3 = true);
        Task<string> GetTmpFileUploadUrl(string filename, string contentType);
        Task SaveTmpFile(string fileName, byte[] fileBytes, string contentType);
        Task<string> GetFilePresignedUrl(string fileName, string contentType, int minutesBeforeExpiry);
    }
}