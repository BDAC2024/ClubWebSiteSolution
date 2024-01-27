using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IOrderRepository
    {
        string SiteUrl { get; }
        Task AddOrUpdateOrder(Order order);
        Task<List<Order>> GetOrders(Season? season = null);
    }
}