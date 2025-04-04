using System.Collections.Generic;
using System;
using AnglingClubWebServices.Interfaces;
using System.Linq;

namespace AnglingClubWebServices.Services
{
    /// <summary>
    /// Groups several utility services together that are not directly concerned with the website
    /// </summary>
    public class UtilityService : IUtilityService
    {
        private bool debugging = false;

        /// <summary>
        /// Calculates the correct list of page numbers to be entered into the MS Word print dialog
        /// to ease the production of membership books/diaries.
        /// </summary>
        /// <param name="numPages">Number of real pages in your document, not including any padding pages at teh end.</param>
        /// <param name="pagesPerSheet">The number of pages (per book) to print per sheet of paper</param>
        /// <param name="printCoverSeparately">Whether to produce 2 lists of pages; one for the front/back cover and one for the rest of the content</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<PrintPaginationSummary> CalcPagesForBookPrinting(int numPages, int pagesPerSheet, bool printCoverSeparately)
        {
            if (numPages % 4 != 0)
            {
                throw new Exception("Sorry; number of pages must be divisible by 4 - aborted");
            }

            List<PrintPaginationSummary> printPages = new List<PrintPaginationSummary>();

            int numSheets = numPages / pagesPerSheet;

            List<int> pageNumbers = new List<int>();

            for (var sheet = 0; sheet < numSheets; sheet++)
            {
                var frontLeft = numPages - sheet * (pagesPerSheet / 2);
                var frontRight = 1 + sheet * (pagesPerSheet / 2);

                pageNumbers.Add(frontLeft);
                pageNumbers.Add(frontRight);
                pageNumbers.Add(frontLeft);
                pageNumbers.Add(frontRight);

                var backLeft = frontRight + 1;
                var backRight = frontLeft - 1;

                pageNumbers.Add(backLeft);
                pageNumbers.Add(backRight);
                pageNumbers.Add(backLeft);
                pageNumbers.Add(backRight);

            }

            int innerContentStart = 0;
            if (printCoverSeparately)
            {
                innerContentStart = pagesPerSheet * 2;

                PrintPaginationSummary coverPage = new PrintPaginationSummary();
                coverPage.Instructions = "Under Settings choose 'Custom Print' then enter the following page numbers: -";
                coverPage.PagesToPrint = string.Join(",", pageNumbers.Take(innerContentStart).ToArray());
                printPages.Add(coverPage);

                debugPrint("Cover:");
                debugPrint(string.Join(",", pageNumbers.Take(innerContentStart).ToArray()));
            }

            PrintPaginationSummary pages = new PrintPaginationSummary();
            pages.Instructions += "Under Settings choose 'Custom Print' then enter the following page numbers: -";
            pages.PagesToPrint = string.Join(",", pageNumbers.Skip(innerContentStart).ToArray());
            printPages.Add(pages);

            debugPrint("Content:");
            debugPrint(string.Join(",", pageNumbers.Skip(innerContentStart).ToArray()));

            return printPages;
        }

        public class PrintPaginationSummary
        {
            public string Instructions { get; set; } = "";
            public string PagesToPrint { get; set; } = "";
        }

        private void debugPrint(string str)
        {
            if (debugging)
            {
                Console.WriteLine(str);
            }
        }
    }
}
