using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IWaterRepository
    {
        Task AddOrUpdateWater(Water water);
        Task UpdateDesc(Water water);
        Task UpdateDirections(Water water);

        Task<List<Water>> GetWaters();
    }
}