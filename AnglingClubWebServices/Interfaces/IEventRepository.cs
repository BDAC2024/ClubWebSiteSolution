using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IEventRepository
    {
        Task AddOrUpdateEvent(ClubEvent clubEVent);
        Task<List<ClubEvent>> GetDayTicketMatches();
        Task<List<ClubEvent>> GetEvents();
    }
}