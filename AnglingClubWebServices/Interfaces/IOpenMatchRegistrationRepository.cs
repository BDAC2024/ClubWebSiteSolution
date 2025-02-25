using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IOpenMatchRegistrationRepository
    {
        Task AddOrUpdateOpenMatchRegistration(OpenMatchRegistration registration);
        Task DeleteOpenMatchRegistration(string id);
        Task<List<OpenMatchRegistration>> GetOpenMatchRegistrations();
    }
}