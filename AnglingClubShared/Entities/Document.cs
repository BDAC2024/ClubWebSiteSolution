using AnglingClubShared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnglingClubShared.Entities
{
    public class DocumentMeta : TableBase
    {
        public string Id { get; set; } = "";
        public DateTime Created { get; set; }
        public string Name { get; set; } = "";
        public string Notes { get; set; } = "";
        public DocumentType DocumentType { get; set; }
    }


}
