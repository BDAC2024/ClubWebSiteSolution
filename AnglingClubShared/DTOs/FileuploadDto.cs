using System;

namespace AnglingClubShared.DTOs
{
    public class FileUploadUrlDto
    {
        /// <summary>
        /// Path or folder structure for the file eg Meetings/Minutes
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// The original name of the  file eg.Minutes_301263.docx
        /// </summary>
        public string Filename { get; set; } = "";

        /// <summary>
        /// The type expressed as MIME type eg application/pdf
        /// </summary>
        public string ContentType { get; set; } = "";
    }

    public class FileUploadUrlResult
    {
        public string UploadUrl { get; set; } = "";
        public string UploadedFileName { get; set; } = "";
    }
}
