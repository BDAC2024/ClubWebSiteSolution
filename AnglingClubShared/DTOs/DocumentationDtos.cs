using System;
using System.Collections.Generic;

namespace AnglingClubShared.DTOs
{
    public class DocumentationListingDto
    {
        public List<string> Folders { get; set; } = new();
        public List<DocumentationFileItemDto> Files { get; set; } = new();
    }

    public class DocumentationFileItemDto
    {
        public string Key { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }

    public class DocumentationUploadUrlRequestDto
    {
        public string FolderPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public bool OverwriteIfExists { get; set; }
    }

    public class DocumentationUploadUrlResultDto
    {
        public bool FileExists { get; set; }
        public string UploadUrl { get; set; } = string.Empty;
        public string UploadedFileName { get; set; } = string.Empty;
    }

    public class CreateDocumentationFolderRequestDto
    {
        public string ParentPath { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
    }
}
