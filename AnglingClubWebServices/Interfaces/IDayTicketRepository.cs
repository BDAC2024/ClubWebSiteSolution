using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IDayTicketRepository
    {
        Task AddOrUpdateTicket(DayTicket ticket);
        Task<List<DayTicket>> GetDayTickets(Season? season = null);
        Task DeleteDayTicket(string id);
    }
}