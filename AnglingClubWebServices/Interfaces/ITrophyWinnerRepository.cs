using AnglingClubShared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface ITrophyWinnerRepository
    {
        Task AddOrUpdateTrophyWinner(TrophyWinner trophyWinner);
        Task<List<TrophyWinner>> GetTrophyWinners();
    }
}