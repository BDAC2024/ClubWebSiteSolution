namespace AnglingClubShared.DTOs
{
    public class DocumentationStoredFileDto
    {
        public string Key { get; set; } = "";
        public DateTime Created { get; set; }
    }

    public class DocumentationCreateFolderRequest
    {
        public string FolderPath { get; set; } = "";
    }

    public class DocumentationUploadUrlRequest
    {
        public string FolderPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "application/octet-stream";
        public bool OverwriteExisting { get; set; }
    }

    public class DocumentationDownloadUrlRequest
    {
        public string Key { get; set; } = "";
    }
}
