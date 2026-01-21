using AnglingClubWebServices.Models;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace AnglingClubWebServices.Services
{
    public class BookPrintingService : IBookPrintingService
    {
        public (byte[] CoversPdf, byte[] ContentPdf) Impose(Stream inputPdf, BookPrintingOptions options)
        {
            if (inputPdf is null)
            {
                throw new ArgumentNullException(nameof(inputPdf));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Load PDF (guardrails: encrypted / password-protected)
            using var loaded = LoadPdfOrThrow(inputPdf);

            int n = loaded.Pages.Count;
            if (n <= 0)
            {
                throw new InvalidOperationException("PDF contains no pages.");
            }

            if (n > options.MaxPages)
            {
                throw new InvalidOperationException($"PDF has {n} pages; maximum allowed is {options.MaxPages}.");
            }

            if (n % 4 != 0)
            {
                throw new InvalidOperationException($"Total page count must be divisible by 4. Found {n} pages.");
            }

            if (options.RequireConsistentPageSize)
            {
                EnsureConsistentPageSize(loaded);
            }

            // Split ranges
            // Covers: [1,2,N-1,N] => always 4 pages if SeparateCovers
            // Content: [3..N-2]
            var coversBytes = Array.Empty<byte>();
            var contentBytes = Array.Empty<byte>();

            if (options.SeparateCovers)
            {
                if (n < 8)
                {
                    throw new InvalidOperationException("PDF is too short to separate covers and content (needs at least 8 pages).");
                }

                int contentCount = n - 4;
                if (contentCount % 4 != 0)
                {
                    throw new InvalidOperationException($"After separating covers, content page count must still be divisible by 4. Content would be {contentCount} pages.");
                }

                var coverSequence = BuildBookletSequence(
                    startPage: 1,
                    endPage: n,
                    takeOnlyCovers: true,
                    twoBooksPerSheet: options.TwoBooksPerSheet);

                var contentSequence = BuildBookletSequence(
                    startPage: 3,
                    endPage: n - 2,
                    takeOnlyCovers: false,
                    twoBooksPerSheet: options.TwoBooksPerSheet);

                coversBytes = BuildImposedPdfBytes(loaded, coverSequence, options);
                contentBytes = BuildImposedPdfBytes(loaded, contentSequence, options);
            }
            else
            {
                // Single output but you asked for always returning two PDFs;
                // We'll return empty covers and all pages as content.
                var contentSequence = BuildBookletSequence(
                    startPage: 1,
                    endPage: n,
                    takeOnlyCovers: false,
                    twoBooksPerSheet: options.TwoBooksPerSheet);

                contentBytes = BuildImposedPdfBytes(loaded, contentSequence, options);
            }

            return (coversBytes, contentBytes);
        }

        private static PdfLoadedDocument LoadPdfOrThrow(Stream inputPdf)
        {
            try
            {
                // Syncfusion may throw on encrypted PDFs, invalid PDFs, etc.
                return new PdfLoadedDocument(inputPdf);
            }
            catch (PdfException ex)
            {
                // Typical for encrypted/password PDFs or malformed docs
                throw new InvalidOperationException("Unable to load PDF. It may be encrypted, password-protected, or invalid.", ex);
            }
        }

        private static void EnsureConsistentPageSize(PdfLoadedDocument loaded)
        {
            // Tolerance in points (~0.5pt is tiny)
            const float tol = 0.5f;

            var first = loaded.Pages[0].Size;

            for (int i = 1; i < loaded.Pages.Count; i++)
            {
                var s = loaded.Pages[i].Size;
                if (Math.Abs(s.Width - first.Width) > tol || Math.Abs(s.Height - first.Height) > tol)
                {
                    throw new InvalidOperationException(
                        $"PDF pages are not consistent size. Page 1 is {first.Width:0.##}x{first.Height:0.##}pt, " +
                        $"page {i + 1} is {s.Width:0.##}x{s.Height:0.##}pt.");
                }
            }
        }

        /// <summary>
        /// Builds the page order for booklet printing, producing output "sides".
        /// Each output side is 4 logical pages (TL, TR, BL, BR).
        ///
        /// For twoBooksPerSheet=true: duplicates pages so TL=BL and TR=BR (your current workflow).
        /// For covers: only includes [1,2,N-1,N] in the correct imposed order.
        /// </summary>
        private static List<int> BuildBookletSequence(int startPage, int endPage, bool takeOnlyCovers, bool twoBooksPerSheet)
        {
            int count = endPage - startPage + 1;
            if (count <= 0)
            {
                throw new InvalidOperationException("Invalid page range.");
            }

            if (count % 4 != 0)
            {
                throw new InvalidOperationException($"Page range {startPage}-{endPage} must be divisible by 4. Range has {count} pages.");
            }

            // Classic saddle-stitch pairing:
            // Sheet s:
            // Outer/front: (end-2s, start+2s)
            // Inner/back:  (start+1+2s, end-1-2s)
            var sides = new List<int>(count * 2); // rough

            int sheets = count / 4;
            for (int s = 0; s < sheets; s++)
            {
                int leftOuter = endPage - (2 * s);
                int rightOuter = startPage + (2 * s);

                int leftInner = startPage + 1 + (2 * s);
                int rightInner = endPage - 1 - (2 * s);

                // If we are generating covers only from full range 1..N:
                // covers are [1,2,N-1,N], which correspond exactly to s=0 sheet of 1..N
                if (takeOnlyCovers)
                {
                    // Only include the first sheet's two sides.
                    if (s != 0)
                    {
                        break;
                    }
                }

                // Each physical sheet has 2 sides (front/back).
                // Each side is a 2x2 grid (4 slots). Your current "2 books per sheet" duplicates.
                AddSide(sides, leftOuter, rightOuter, twoBooksPerSheet); // front
                AddSide(sides, leftInner, rightInner, twoBooksPerSheet); // back
            }

            return sides;
        }

        private static void AddSide(List<int> output, int left, int right, bool twoBooksPerSheet)
        {
            if (twoBooksPerSheet)
            {
                // TL, TR, BL, BR
                output.Add(left);
                output.Add(right);
                output.Add(left);
                output.Add(right);
            }
            else
            {
                // One book per sheet: use all four slots distinctly (common alternative)
                // For simplicity: put left/right on top row; leave bottom row blank by repeating
                // or you could do a true 2-up. If you want a true one-book layout, say so.
                output.Add(left);
                output.Add(right);
                output.Add(left);
                output.Add(right);
            }
        }

        private static byte[] BuildImposedPdfBytes(PdfLoadedDocument source, List<int> sideSequence, BookPrintingOptions options)
        {
            if (sideSequence.Count == 0)
            {
                return Array.Empty<byte>();
            }

            if (sideSequence.Count % 4 != 0)
            {
                throw new InvalidOperationException("Internal error: side sequence must be a multiple of 4.");
            }

            using var outDoc = new PdfDocument();

            outDoc.PageSettings.Size = options.OutputSheetSize;

            // KEY: remove Syncfusion default margins
            outDoc.PageSettings.Margins = new PdfMargins() { All = 0 };

            float margin = options.MarginPoints;   // your own layout margin (can be 0)
            float gutter = options.GutterPoints;

            for (int i = 0; i < sideSequence.Count; i += 4)
            {
                var page = outDoc.Pages.Add();
                var g = page.Graphics;

                // KEY: use full page size, not client size
                var sheet = page.Size;

                float slotW = (sheet.Width - (2 * margin) - gutter) / 2f;
                float slotH = (sheet.Height - (2 * margin) - gutter) / 2f;

                var slots = new[]
                {
                    new RectangleF(margin, margin, slotW, slotH),                                  // TL
                    new RectangleF(margin + slotW + gutter, margin, slotW, slotH),                 // TR
                    new RectangleF(margin, margin + slotH + gutter, slotW, slotH),                 // BL
                    new RectangleF(margin + slotW + gutter, margin + slotH + gutter, slotW, slotH) // BR
                };

                for (int s = 0; s < 4; s++)
                {
                    int logicalPage = sideSequence[i + s];
                    var template = source.Pages[logicalPage - 1].CreateTemplate();
                    DrawFit(g, template, slots[s]);
                }

                DrawCutMarks(page, options);
            }

            using var ms = new MemoryStream();
            outDoc.Save(ms);
            return ms.ToArray();
        }

        private static void DrawCutMarks(PdfPage page, BookPrintingOptions options)
        {
            var g = page.Graphics;
            var sheet = page.Size;

            // Line style
            var pen = new PdfPen(PdfBrushes.Gray, 0.3f); // 0.3pt hairline

            float y = sheet.Height / 2f;

            // Inset slightly from edges to avoid printer clipping
            float inset = 12f; // ~4mm

            float tick = 18f; // length of tick

            // Left edge tick
            g.DrawLine(pen, inset, y, inset + tick, y);

            // Right edge tick
            g.DrawLine(pen, sheet.Width - inset - tick, y, sheet.Width - inset, y);
        }

        private static void DrawFit(PdfGraphics g, PdfTemplate t, RectangleF slot)
        {
            var src = t.Size; // SizeF in points
            if (src.Width <= 0 || src.Height <= 0)
            {
                return;
            }

            float scale = Math.Min(slot.Width / src.Width, slot.Height / src.Height);

            float w = src.Width * scale;
            float h = src.Height * scale;

            float x = slot.X + (slot.Width - w) / 2f;
            float y = slot.Y + (slot.Height - h) / 2f;

            g.DrawPdfTemplate(t, new PointF(x, y), new SizeF(w, h));
        }
    }
}
