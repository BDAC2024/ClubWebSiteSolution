using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Services
{
    public interface ICurrentUserService
    {
        ClientMemberDto User { get; set; }
    }
}
