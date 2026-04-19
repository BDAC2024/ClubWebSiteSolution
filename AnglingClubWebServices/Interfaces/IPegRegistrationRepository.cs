using AnglingClubShared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IPegRegistrationRepository
    {
        Task AddOrUpdatePegRegistration(PegRegistration registration);
        Task<List<PegRegistration>> GetPegRegistrations();
        Task DeletePegRegistration(string id);
    }
}
