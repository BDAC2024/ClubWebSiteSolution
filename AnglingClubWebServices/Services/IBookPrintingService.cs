using AnglingClubWebServices.Models;
using System.IO;

namespace AnglingClubWebServices.Services
{
    public interface IBookPrintingService
    {
        (byte[] CoversPdf, byte[] ContentPdf) Impose(Stream inputPdf, BookPrintingOptions options);
    }
}