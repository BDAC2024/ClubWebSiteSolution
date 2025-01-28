using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public MemberDto User { get; set; } = new MemberDto();
    }
}
