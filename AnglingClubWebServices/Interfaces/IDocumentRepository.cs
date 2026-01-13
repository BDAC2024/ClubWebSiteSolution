using AnglingClubShared.Entities;
using Syncfusion.DocIO.DLS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddOrUpdateDocument(DocumentMeta file);
        Task<List<DocumentMeta>> Get();
        Task<WordDocument> GetWordDocument(string fileName);
        Task<string> GetDocumentUploadUrl(string filename, string contentType);
        Task DeleteDocument(string id);
    }
}