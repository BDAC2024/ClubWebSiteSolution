using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IGuestTicketRepository
    {
        Task AddOrUpdateTicket(GuestTicket guestTicket);
        Task<List<GuestTicket>> GetGuestTickets(Season? season = null);
        Task DeleteGuestTicket(string id);
    }
}