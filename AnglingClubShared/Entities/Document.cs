using AnglingClubShared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnglingClubShared.Entities
{
    public class DocumentMeta : TableBase
    {
        public string Id { get; set; } = "";
        public DateTime Created { get; set; }
        /// <summary>
        /// Membership number of the person who uploaded the document
        /// </summary>
        public int UploadedBy { get; set; } 
        public string StoredFileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Notes { get; set; } = "";
        public DocumentType DocumentType { get; set; }
    }


}
