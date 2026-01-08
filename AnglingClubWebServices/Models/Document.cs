using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using System;

namespace AnglingClubWebServices.Models
{
    public class DocumentMeta : TableBase
    {
        public string Id { get; set; } = "";
        public DateTime Created { get; set; }
        public string Name { get; set; } = "";
        public DocumentType DocumentType { get; set; }
    }


}
