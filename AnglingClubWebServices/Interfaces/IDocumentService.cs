using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentService
    {
        public Task<byte[]> GenerateWatermarkedPdfFromWordDocument(string fileName, string watermarkText, string footerText, CancellationToken ct);
        Task SaveDocument(DocumentMeta docItem, int createdByMember);
        Task<List<DocumentListItem>> GetDocuments(DocumentSearchRequest req);

    }
}