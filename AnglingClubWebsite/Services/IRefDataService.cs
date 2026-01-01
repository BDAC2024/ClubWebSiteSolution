using AnglingClubShared.Models;

namespace AnglingClubWebsite.Services
{
    public interface IRefDataService
    {
        Task<ReferenceData?> ReadReferenceData();
    }
}