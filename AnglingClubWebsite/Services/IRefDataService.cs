using AnglingClubShared.Models;

namespace AnglingClubWebsite.Services
{
    public interface IRefDataService
    {
        Task InitializeAsync();
        Task<ReferenceData?> ReadReferenceData();
    }
}