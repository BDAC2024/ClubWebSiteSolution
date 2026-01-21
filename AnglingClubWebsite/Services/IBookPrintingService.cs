using AnglingClubShared.DTOs;
using Syncfusion.Blazor.Inputs;

namespace AnglingClubWebsite.Services
{
    public interface IBookPrintingService
    {
        Task<BookPrintingResult?> GetPrintReadyPDFs(UploadFiles? file);
    }
}