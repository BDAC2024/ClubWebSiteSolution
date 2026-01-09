using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Services
{
    public interface IAboutService
    {
        Task<AboutDto?> GetAboutInfo();
    }
}