using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public ClientMemberDto User { get; set; } = new ClientMemberDto();
    }
}
