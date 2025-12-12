using AnglingClubShared.Entities;
using AnglingClubShared.Models.Auth;
using AnglingClubWebServices.Models;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);

        Task<Member> GetByKey(string key);
        Task<Member> GetAuthorisedUserByKey(string key);

        string GetDeveloperName();

    }
}
