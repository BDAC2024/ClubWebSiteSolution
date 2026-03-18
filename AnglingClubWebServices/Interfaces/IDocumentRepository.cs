using AnglingClubShared.Entities;
using AnglingClubWebServices.Models;
using Syncfusion.DocIO.DLS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddOrUpdateDocument(DocumentMeta file);
        Task AddOrUpdateAndIndexDocument(DocumentMeta file);
        Task<List<DocumentMeta>> Get();
        Task<DocumentMeta> GetById(string docId);
        Task<WordDocument> GetWordDocument(string fileName);
        Task<string> GetDocumentUploadUrl(string filename, string contentType);
        Task DeleteDocument(string id);
        Task<string> GetFilePresignedUrl(string storedFileName, string returnedFileName, int minutesBeforeExpiry);
        Task<string> GetRawText(DocumentMeta doc);
        Task<List<StoredFileMeta>> GetFilesByPrefix(string prefix);
        Task<bool> FileExists(string key);
        Task CreateFolderMarker(string folderPath);
    }
}