using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITmpFileRepository
    {
        Task AddOrUpdateTmpFile(TmpFile file);
        Task<List<TmpFile>> GetTmpFiles(bool loadFile = true);
        Task<TmpFile> GetTmpFile(string id);
        Task DeleteTmpFile(string id);
    }
}