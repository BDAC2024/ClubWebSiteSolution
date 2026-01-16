using Syncfusion.DocIO.DLS;
using System.IO;
using System.Text.RegularExpressions;

namespace AnglingClubWebServices.Helpers
{
    public static class WordTextExtractor
    {
        public static string ExtractAndNormalizeText(Stream docStream, Syncfusion.DocIO.FormatType formatType)
        {
            using var document = new WordDocument(docStream, formatType);

            // DocIO exposes several ways to read text; the simplest is GetText().
            var raw = document.GetText() ?? string.Empty;

            // Normalize: collapse whitespace, trim, lower-case (optional).
            var normalized = Regex.Replace(raw, @"\s+", " ").Trim();

            return normalized;
        }
    }
}
