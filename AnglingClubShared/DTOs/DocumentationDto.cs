using System;
using System.Collections.Generic;

namespace AnglingClubShared.DTOs
{
    public class DocumentationFolderTreeDto
    {
        public List<string> FolderPaths { get; set; } = new List<string>();
    }

    public class DocumentationFileItemDto
    {
        public string Key { get; set; } = "";
        public string FileName { get; set; } = "";
        public DateTime Created { get; set; }
        public long SizeBytes { get; set; }
    }

    public class DocumentationUploadUrlRequestDto
    {
        public string FolderPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "application/octet-stream";
        public bool Overwrite { get; set; } = false;
    }

    public class DocumentationUploadUrlResultDto
    {
        public bool RequiresOverwriteConfirmation { get; set; } = false;
        public bool AlreadyExists { get; set; } = false;
        public string UploadUrl { get; set; } = "";
        public string UploadedFileName { get; set; } = "";
    }

    public class DocumentationCreateFolderRequestDto
    {
        public string ParentFolderPath { get; set; } = "";
        public string FolderName { get; set; } = "";
    }
}
