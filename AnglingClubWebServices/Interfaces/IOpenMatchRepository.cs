using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IOpenMatchRepository
    {
        Task AddOrUpdateOpenMatch(OpenMatch match);
        Task DeleteOpenMatch(string id);
        Task<List<OpenMatch>> GetOpenMatches();
    }
}