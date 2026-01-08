using System.Threading;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDocumentService
    {
        public Task<byte[]> GenerateWatermarkedPdfFromWordDocument(string fileName, string watermarkText, string footerText, CancellationToken ct);

    }
}