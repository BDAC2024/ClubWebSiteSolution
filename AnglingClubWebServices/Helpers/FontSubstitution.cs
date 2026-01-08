using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace AnglingClubWebServices
{
    internal static class DocioFontSubstitution
    {
        private static readonly ConcurrentDictionary<string, byte[]> _fontCache = new();

        private static MemoryStream OpenFont(string folderName, string fileName)
        {
            var bytes = _fontCache.GetOrAdd($"{folderName}/{fileName}", _ =>
            {
                var path = Path.Combine(AppContext.BaseDirectory, folderName, fileName);
                return File.ReadAllBytes(path);
            });

            // Return a NEW stream each time (important)
            return new MemoryStream(bytes, writable: false);
        }

        public static void AttachLatoSubstitution(WordDocument doc, string folderName = "LatoFont")
        {
            doc.FontSettings.SubstituteFont += (s, e) =>
            {
                var file = e.FontStyle switch
                {
                    Syncfusion.Drawing.FontStyle.Bold =>
                                            "Lato-Bold.ttf",

                    Syncfusion.Drawing.FontStyle.Italic =>
                        "Lato-Italic.ttf",

                    Syncfusion.Drawing.FontStyle.Bold | Syncfusion.Drawing.FontStyle.Italic =>
                        "Lato-BoldItalic.ttf",

                    _ =>
                        "Lato-Regular.ttf"
                };

                e.AlternateFontStream = OpenFont(folderName, file);
            };
        }
    }
}
