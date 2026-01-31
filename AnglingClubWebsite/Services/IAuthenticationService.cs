using AnglingClubShared.DTOs;
using AnglingClubShared.Models.Auth;

namespace AnglingClubWebsite.Services
{
    public interface IAuthenticationService
    {
        event Action<string?>? LoginChange;

        //ValueTask<string> GetJwtAsync();
        Task<bool> LoginAsync(AuthenticateRequest model, bool rememberMe = true);
        Task LogoutAsync();
        //Task<bool> RefreshAsync();
        Task<bool> isLoggedIn();
        Task<bool> sessionExpired();
        Task<bool> PinResetRequest(int membershipNumber);

        Task<ClientMemberDto> GetCurrentUser();
    }
}
