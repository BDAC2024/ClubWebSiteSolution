using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IGlobalService
    {
        Season GetStoredSeason(Season defaultIfEmpty);
        void SetStoredSeason(Season season);

    }
}