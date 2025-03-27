using System.Collections.Generic;
using System;
using AnglingClubWebServices.Interfaces;

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
        /// <param name="pagesPerSide">The number of pages to print per side</param>
        /// <param name="printCoverSeparately">Whether to produce 2 lists of pages; one for the front/back cover and one for the rest of the content</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<PrintPaginationSummary> CalcPagesForBookPrinting(int numPages, int pagesPerSide, bool printCoverSeparately)
        {
            var doubleSided = true;

            if (numPages % 4 != 0)
            {
                throw new Exception("Sorry; number of pages must be divisible by 4 - aborted");
            }

            var pagesPerSheet = pagesPerSide * (doubleSided ? 2 : 1);
            var dummyCount = numPages % pagesPerSheet;
            var sheetsRequired = (numPages + dummyCount) / pagesPerSheet;

            var fp = 1;
            var lp = numPages;

            var row1Pages = new string[sheetsRequired * 2];
            var row2Pages = new string[sheetsRequired * 2];

            var row11CreationIndex = 0;
            var row2CreationIndex = 0;

            // Row 1 first
            for (int sheet = 0; sheet < sheetsRequired; sheet++)
            {
                for (int side = 0; side < 2; side++)
                {
                    string v = "";
                    if (side == 0)
                    {
                        v = $"{lp--},{fp++},";
                    }
                    if (side == 1)
                    {
                        v = $"{fp++},{lp--},";
                    }
                    row1Pages[row11CreationIndex++] = v;
                    debugPrint($"Row2 : sheet{sheet}, side{side} = {v}");
                }
            }

            // Row 2 next
            for (int sheet = 0; sheet < sheetsRequired; sheet++)
            {
                for (int side = 0; side < 2; side++)
                {
                    string v = "";

                    if (side == 0)
                    {
                        v = $"{lp--},{fp++},";
                    }
                    if (side == 1)
                    {
                        v = $"{fp++},{lp--},";
                    }
                    row2Pages[row2CreationIndex++] = v;
                    debugPrint($"Row1 : sheet{sheet}, side{side} = {v}");
                }
            }

            // Add dummy/blank pages if required
            if (dummyCount > 0 || (dummyCount == 0 && printCoverSeparately))
            {
                row2Pages[sheetsRequired * 2 - 1] = $"{numPages + 1},{numPages + 1},";
                row2Pages[sheetsRequired * 2 - 2] = $"{numPages + 1},{numPages + 1},";
            }

            List<PrintPaginationSummary> printPages = new List<PrintPaginationSummary>();

            var row11Index = 0;
            var row2Index = 0;

            if (printCoverSeparately)
            {
                PrintPaginationSummary coverPage = new PrintPaginationSummary();

                for (int side = 0; side < 2; side++)
                {
                    coverPage.PagesToPrint += $"{row1Pages[row11Index]}{row1Pages[row11Index]}";
                    row11Index++;
                }

                coverPage.Instructions = "For the front cover; Under Settings choose 'Custom Print' then enter the following page numbers: -";
                coverPage.PagesToPrint = (coverPage.PagesToPrint + coverPage.PagesToPrint).Substring(0, coverPage.PagesToPrint.Length - 1); // drop the last comma
                printPages.Add(coverPage);
            }

            PrintPaginationSummary summary = new PrintPaginationSummary();

            row11Index = printCoverSeparately ? 2 : 0;
            row2Index = 0;

            var loopCount = printCoverSeparately ? row2Pages.Length - 2 : row2Pages.Length;

            for (int i = 0; i < loopCount; i++)
            {
                summary.PagesToPrint += $"{row1Pages[row11Index++]}{row2Pages[row2Index++]}";
            }

            summary.Instructions = printCoverSeparately ? "For the inside pages; " : "";
            summary.Instructions += "Under Settings choose 'Custom Print' then enter the following page numbers: -";
            summary.PagesToPrint = summary.PagesToPrint.Substring(0, summary.PagesToPrint.Length - 1); // drop the last comma
            printPages.Add(summary);

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
