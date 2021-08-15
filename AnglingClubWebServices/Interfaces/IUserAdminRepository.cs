using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IUserAdminRepository
    {
        Task AddOrUpdateUserAdmin(UserAdminContact userAdmin);
        Task<List<UserAdminContact>> GetUserAdmins();
        Task DeleteUserAdmin(string id);
    }
}