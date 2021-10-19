using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IBackupRepository
    {
        Task Restore(List<BackupLine> backupLines, string restoreToDomain);
        Task<List<BackupLine>> Backup(int itemsToBackup);
        Task ClearDb(string domainToClear);

    }
}
