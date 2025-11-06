using System;

namespace AnglingClubWebServices.Models
{
    public class StoredFileMeta
    {
        public string Id { get; set; } = "";
        public DateTime Created { get; set; }
    }

    public class StoredFile : StoredFileMeta
    {
        public string Content { get; set; } = null;
    }

}
