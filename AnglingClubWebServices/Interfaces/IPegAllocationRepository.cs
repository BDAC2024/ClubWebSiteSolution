using AnglingClubShared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IPegAllocationRepository
    {
        Task AddOrUpdatePegAllocation(PegAllocation allocation);
        Task<List<PegAllocation>> GetPegAllocations();
        Task DeletePegAllocation(string id);
    }
}
