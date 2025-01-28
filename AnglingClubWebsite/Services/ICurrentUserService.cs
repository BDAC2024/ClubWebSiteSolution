using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Services
{
    public interface ICurrentUserService
    {
        MemberDto User { get; set; }
    }
}
