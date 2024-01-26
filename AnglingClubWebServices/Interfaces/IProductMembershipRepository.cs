using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IProductMembershipRepository
    {
        Task AddOrUpdateProductMembership(ProductMembership membership);
        Task<List<ProductMembership>> GetProductMemberships();
        Task DeleteProductMembership(string id);
    }
}