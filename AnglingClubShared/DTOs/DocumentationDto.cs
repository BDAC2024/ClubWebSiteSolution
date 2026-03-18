namespace AnglingClubShared.DTOs
{
    public class DocumentationBucketItemDto
    {
        public string Key { get; set; } = "";
        public DateTime LastModifiedUtc { get; set; }
        public bool IsFolderPlaceholder { get; set; }
    }

    public class DocumentationListResponse
    {
        public List<DocumentationBucketItemDto> Items { get; set; } = new();
    }

    public class CreateDocumentationFolderRequest
    {
        public string FolderPath { get; set; } = "";
    }

    public class DocumentationUploadUrlRequest
    {
        public string FolderPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public bool OverwriteExisting { get; set; }
    }

    public class DocumentationUploadUrlResponse
    {
        public bool FileAlreadyExists { get; set; }
        public string UploadUrl { get; set; } = "";
        public string StorageKey { get; set; } = "";
    }
}
