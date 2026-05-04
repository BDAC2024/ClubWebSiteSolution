using AnglingClubShared.Entities;
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IClubEventService
    {
        Task<List<ClubEvent>?> ReadEventsForSeason(Season season);
        Task<List<ClubEvent>?> GetPresentationNightForSeason(Season season);
    }
}