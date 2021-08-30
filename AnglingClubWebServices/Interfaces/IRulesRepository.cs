using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IRulesRepository
    {
        Task AddOrUpdateRules(Rules rules);
        Task<List<Rules>> GetRules();
    }
}