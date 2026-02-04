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

    }
}