using System;

namespace AnglingClubShared.Entities
{
    public abstract class TableBase
    {
        public string DbKey { get; set; } = "";

        public bool IsNewItem
        {
            get
            {
                return string.IsNullOrEmpty(DbKey);
            }
        }

        public string GenerateDbKey(string idPrefix)
        {
            return $"{idPrefix}:{Guid.NewGuid().ToString()}";
        }
    }
}
