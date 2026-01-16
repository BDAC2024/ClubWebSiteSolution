using AnglingClubShared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultRepository
    {
        Task AddOrUpdateMatchResult(MatchResult result);

        Task<List<MatchResult>> GetMatchResults(string matchId);
        Task<List<MatchResult>> GetAllMatchResults();
    }
}