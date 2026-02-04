using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.Models;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AutoMapper;
using Microsoft.Extensions.Options;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly AuthOptions _authOptions;
        private readonly IMemberRepository _memberRepository;
        private readonly ITmpFileRepository _tmpFileRepository;
        private readonly IMapper _mapper;

        public DocumentService(IDocumentRepository documentRepository, IOptions<AuthOptions> authOpts, IMemberRepository memberRepository, IMapper mapper, ITmpFileRepository tmpFileRepository)
        {
            _documentRepository = documentRepository;
            _authOptions = authOpts.Value;
            _memberRepository = memberRepository;
            _mapper = mapper;
            _tmpFileRepository = tmpFileRepository;
        }

        public async Task<string> GetReadOnlyMinutesUrl(string id, Member user, CancellationToken ct)
        {
            //var doc = await _documentRepository.GetById(id + "ZZZZ");
            var doc = await _documentRepository.GetById(id);

            var effectiveSeason = EnumUtils.SeasonForDate(doc.Created).Value;
            var member = (await _memberRepository.GetMembers(effectiveSeason)).FirstOrDefault(x => x.MembershipNumber == user.MembershipNumber);

            var name = member != null ? member.Name : "";
            var fileName = doc.StoredFileName;
            var requestedAt = DateTime.Now;

            var pdfBytes = await generateWatermarkedPdfFromWordDocument(
                fileName: fileName,
                watermarkText: $"COPY FOR {name.ToUpper()}",
                footerText: $"Requested by {name} on {requestedAt.ToString("dd MMM yyyy")} at {requestedAt.ToString("hh:mm tt")}",
                ct: ct);

            var pdfFileName = Path.ChangeExtension(fileName, ".pdf");

            await _tmpFileRepository.SaveTmpFile(pdfFileName, pdfBytes, "application/pdf");

            var url = await _tmpFileRepository.GetFilePresignedUrl(pdfFileName, SharedConstants.MINUTES_TO_EXPIRE_LINKS, "application/pdf");

            return url;
        }

        public async Task<string> Download(string id, Member user)
        {
            var doc = (await _documentRepository.Get()).SingleOrDefault(x => x.DbKey == id);

            if (doc == null)
            {
                throw new AppNotFoundException("Document could not be found.");
            }

            if (doc.DocumentType == DocumentType.MeetingMinutes && !user.Secretary)
            {
                throw new AppForbiddenException("Only club secretaries can download meeting minutes.");
            }

            var url = await _documentRepository.GetFilePresignedUrl(doc.StoredFileName, doc.OriginalFileName, SharedConstants.MINUTES_TO_EXPIRE_LINKS);

            return url;
        }
        private async Task<byte[]> generateWatermarkedPdfFromWordDocument(
            string fileName,
            string watermarkText,
            string footerText,
            CancellationToken ct)
        {
            using (var wordDoc = await _documentRepository.GetWordDocument(fileName))
            {
                // 3) Convert Word -> PDF
                using var renderer = new DocIORenderer();
                using PdfDocument pdf = renderer.ConvertToPDF(wordDoc);

                // 4) Watermark all pages
                ApplyWatermarkOnTop(pdf, watermarkText, footerText);
                return outputStream(pdf);
            }
        }

        private byte[] outputStream(PdfDocument sourcePdf)
        {
            // Apply security
            PdfSecurity security = sourcePdf.Security;

            // Encrypt the PDF
            security.KeySize = PdfEncryptionKeySize.Key256Bit;

            // Optional passwords
            security.OwnerPassword = _authOptions.AuthSecretKey;

            // Disable copy/extract
            security.Permissions = PdfPermissionsFlags.Print;
            // Note: Copy, Extract, Accessibility are omitted

            using var output = new MemoryStream();
            sourcePdf.Save(output);
            var bytes = output.ToArray();

            return bytes;

        }


        private void ApplyWatermarkOnTop(PdfDocument pdf, string watermarkText, string footerText)
        {
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 32f, PdfFontStyle.Bold);
            var brush = new PdfSolidBrush(new PdfColor(120, 120, 120));
            var format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            for (int i = 0; i < pdf.Pages.Count; i++)
            {
                var page = pdf.Pages[i];
                var g = page.Graphics;

                var state = g.Save();

                // Syncfusion 32.x: set transparency on the graphics
                g.SetTransparency(0.25f); // 0.0 = invisible, 1.0 = opaque

                g.TranslateTransform(page.Size.Width / 2f, page.Size.Height / 2f);
                g.RotateTransform(-45);

                g.DrawString(watermarkText, font, brush, 0f, 0f, format);
                g.Restore(state);

                // Optional: footer microtext behind as well
                var smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
                g.DrawString(footerText, smallFont, brush, 10f, page.Size.Height - 15f);

            }
        }

        /// <summary>
        /// Not currently used but may be in future
        /// </summary>
        /// <param name="sourcePdf"></param>
        /// <param name="watermarkText"></param>
        /// <returns></returns>
        public PdfDocument WatermarkBehindByRebuilding(PdfDocument sourcePdf, string watermarkText)
        {
            // 1) Save the generated PDF into memory
            using var srcStream = new MemoryStream();
            sourcePdf.Save(srcStream);
            srcStream.Position = 0;

            // 2) Load as "existing PDF"
            using var loaded = new PdfLoadedDocument(srcStream);

            // 3) Create a fresh output PDF
            var output = new PdfDocument();

            // Ensure no margins interfere with page sizing/positioning
            output.PageSettings.Margins = new PdfMargins
            {
                Left = 0,
                Right = 0,
                Top = 0,
                Bottom = 0
            };

            var wmFont = new PdfStandardFont(PdfFontFamily.Helvetica, 36f, PdfFontStyle.Bold);
            var wmBrush = new PdfSolidBrush(new PdfColor(200, 200, 200)); // light gray
            var format = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);

            for (int i = 0; i < loaded.Pages.Count; i++)
            {
                var srcPage = loaded.Pages[i]; // PdfLoadedPageBase / PdfPageBase
                var pageSize = srcPage.Size;

                // Create output page with identical size
                var outPage = output.Pages.Add();
                var g = outPage.Graphics;

                // --- Draw watermark FIRST (so it is behind) ---
                var state = g.Save();
                g.TranslateTransform(pageSize.Width / 2f, pageSize.Height / 2f);
                g.RotateTransform(-45);
                g.DrawString(watermarkText, wmFont, wmBrush, 0f, 0f, format);
                g.Restore(state);

                // Optional: footer microtext behind as well
                var smallFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
                g.DrawString(watermarkText, smallFont, wmBrush, 10f, pageSize.Height - 15f);

                // --- Draw original content SECOND (on top of watermark) ---
                // Create a template from the original page and paint it at (0,0)
                PdfTemplate template = srcPage.CreateTemplate();
                g.DrawPdfTemplate(template, new Syncfusion.Drawing.PointF(0, 0));
            }

            return output; // caller should Dispose() this
        }

        /// <summary>
        /// Get documents and optionally search
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<List<DocumentListItem>> GetDocuments(DocumentSearchRequest req)
        {
            var searchText = req.SearchText?.ToLowerInvariant();

            var items = new List<DocumentListItem>();

            var members = await _memberRepository.GetMembers((Season?)EnumUtils.CurrentSeason());

            var dbItems = (await _documentRepository.Get()).Where(x => x.DocumentType == req.DocType);

            if (searchText.IsNullOrEmpty())
            {
                // Return all if no search specified
                items = _mapper.Map<List<DocumentListItem>>(dbItems);
            }
            else
            {
                // Search both the raw text of the documents and the Notes field
                var searchableItems = _mapper.Map<List<SearchableDocument>>(dbItems);

                foreach (var item in searchableItems)
                {
                    if (item.Searchable)
                    {
                        item.RawContent = await _documentRepository.GetRawText(item);
                    }
                }

                // Now search
                searchableItems = searchableItems.Where(x =>
                    (x.Notes != null && x.Notes.ToLower().Contains(searchText)) ||
                    (x.RawContent != null && x.RawContent.ToLower().Contains(searchText))
                    ).ToList();

                items = _mapper.Map<List<DocumentListItem>>(searchableItems);
            }

            foreach (var item in items)
            {
                item.UploadedBy = members.First(x => x.MembershipNumber == item.UploadedByMembershipNumber).Name;
            }

            return items;
        }

        public async Task SaveDocument(DocumentMeta docItem, int createdByMember)
        {
            try
            {
                docItem.UploadedByMembershipNumber = createdByMember;
                if (docItem.Searchable)
                {
                    await _documentRepository.AddOrUpdateAndIndexDocument(docItem);
                }
                else
                {
                    await _documentRepository.AddOrUpdateDocument(docItem);
                }

            }
            catch (Exception ex)
            {
                var detailedEx = new Exception("Save failed for for document" +
                    JsonSerializer.Serialize(docItem), ex);

                throw new AppValidationException("Failed to save document", "", detailedEx);
            }
        }
    }
}
