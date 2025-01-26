using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;
using System.Threading.Tasks;

namespace AnglingClubWebsite.Services
{
    public interface IAuthenticationService
    {
        event Action<string?>? LoginChange;

        //ValueTask<string> GetJwtAsync();
        Task<bool> LoginAsync(AuthenticateRequest model);
        Task LogoutAsync();
        //Task<bool> RefreshAsync();
        Task<bool> isLoggedIn();

        Task<MemberDto?> GetCurrentUser();
    }
}
