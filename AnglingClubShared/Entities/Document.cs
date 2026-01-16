using AnglingClubShared.Enums;

namespace AnglingClubShared.Entities
{
    public class DocumentMeta : TableBase
    {
        public DateTime Created { get; set; }
        /// <summary>
        /// Membership number of the person who uploaded the document
        /// </summary>
        public int UploadedByMembershipNumber { get; set; }
        public string StoredFileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Notes { get; set; } = "";
        public DocumentType DocumentType { get; set; }
        public bool Searchable { get; set; } = false;
    }

    public class DocumentListItem : DocumentMeta
    {
        public string UploadedBy { get; set; } = "";
    }

    public class DocumentSearchRequest
    {
        public DocumentType DocType { get; set; }
        public string SearchText { get; set; } = "";
    }

    public class SearchableDocument : DocumentMeta
    {
        public string RawContent { get; set; } = "";
    }

}
