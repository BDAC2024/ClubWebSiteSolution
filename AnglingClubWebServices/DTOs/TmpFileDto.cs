using System;

namespace AnglingClubWebServices.DTOs
{
    public class TmpFileUploadUrlDto
    {
        public string Filename { get; set; } = "";
        public string ContentType { get; set; } = "";
    }

    public class TmpFileUploadUrlResult
    {
        public string UploadUrl { get; set; }
        public string UploadedFileName { get; set; }
    }
}
