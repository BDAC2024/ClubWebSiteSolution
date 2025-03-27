using AnglingClubWebServices.Services;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IUtilityService
    {
        List<UtilityService.PrintPaginationSummary> CalcPagesForBookPrinting(int numPages, int pagesPerSide, bool printCoverSeparately);
    }
}