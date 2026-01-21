using Syncfusion.Drawing;
using Syncfusion.Pdf;

namespace AnglingClubWebServices.Models
{
    public sealed class BookPrintingOptions
    {
        public bool SeparateCovers { get; init; } = true;

        /// <summary>
        /// Guardrail: maximum allowed pages in the uploaded PDF.
        /// </summary>
        public int MaxPages { get; init; } = 200;

        /// <summary>
        /// Guardrail: reject if pages are not the same size (within tolerance).
        /// </summary>
        public bool RequireConsistentPageSize { get; init; } = true;

        /// <summary>
        /// Output sheet size; most people want A4.
        /// </summary>
        public SizeF OutputSheetSize { get; init; } = PdfPageSize.A4;

        /// <summary>
        /// Margin and gutter are in PDF points (1 point = 1/72 inch).
        /// Defaults are conservative to avoid clipping on home printers.
        /// </summary>
        public float MarginPoints { get; init; } = 18f; // ~6.35mm
        public float GutterPoints { get; init; } = 12f; // ~4.23mm

        /// <summary>
        /// If true, duplicates (two books per sheet) are produced: TL=BL and TR=BR.
        /// If false, you get one book per sheet (still 4-up), no duplication.
        /// </summary>
        public bool TwoBooksPerSheet { get; init; } = true;
    }

}
