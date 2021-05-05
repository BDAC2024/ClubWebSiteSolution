using AnglingClubWebServices.Models;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IWaterRepository
    {
        Task AddOrUpdateWater(Water water);
    }
}