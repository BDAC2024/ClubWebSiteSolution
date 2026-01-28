using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;

namespace AnglingClubShared.Entities
{
    public class DocumentMeta : TableBase
    {
        public DateTime Created
        {
            get; set;
        }
        /// <summary>
        /// Membership number of the person who uploaded the document
        /// </summary>
        public int UploadedByMembershipNumber
        {
            get; set;
        }
        /// <summary>
        /// Includes the path within the storage bucket
        /// </summary>
        public string StoredFileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Notes { get; set; } = "";
        public DocumentType DocumentType
        {
            get; set;
        }
        public bool Searchable { get; set; } = false;
    }

    public class DocumentMetaDTO : DocumentMeta
    {
        public DateTimeOffset CreatedOffset
        {
            get; set;
        }
    }

    public class DocumentListItem : DocumentMeta
    {
        public string UploadedBy { get; set; } = "";
    }

    public class DocumentSearchRequest
    {
        public DocumentType DocType
        {
            get; set;
        }
        public string SearchText { get; set; } = "";
    }

    public class SearchableDocument : DocumentMeta
    {
        public string RawContent { get; set; } = "";
    }

    public static class DocumentExtensions
    {
        public static string StoragePath(this DocumentMeta value)
        {
            var docPath = "";

            switch (value.DocumentType)
            {
                case DocumentType.MeetingMinutes:
                    docPath = "Meetings/Minutes";
                    break;

                default:
                    var errMsg = $"DocumentType.StoragePath: Unsupported document type {value}";
                    var ex = new ArgumentOutOfRangeException(errMsg);
                    throw ex;
            }

            return docPath;

        }

        /// <summary>
        /// Includes the path in the bucket
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StorageSearchFilename(this DocumentMeta value)
        {
            const string _searchDataPath = "Search";

            if (value.StoredFileName.IsNullOrEmpty())
            {
                throw new ArgumentOutOfRangeException("Document must already have been saved before getting the search filename");
            }

            var docPath = "";
            var filename = value.StoredFileName.Contains('/') ? Path.GetFileName(value.StoredFileName) : value.StoredFileName;

            switch (value.DocumentType)
            {
                case DocumentType.MeetingMinutes:
                    docPath = $"{value.StoragePath()}/{_searchDataPath}";
                    docPath = string.IsNullOrWhiteSpace(filename) ? docPath : $"{docPath}/{filename}";
                    break;

                default:
                    var errMsg = $"DocumentType.StorageSearchPath: Unsupported document type {value}";
                    var ex = new ArgumentOutOfRangeException(errMsg);
                    throw ex;
            }

            return docPath;

        }



    }

}
