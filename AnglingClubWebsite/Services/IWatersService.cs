using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;

namespace AnglingClubWebsite.Services
{
    public interface IWatersService
    {
        Task<List<WaterOutputDto>?> ReadWaters();
        //Task DeleteNewsItem(string id);
        //Task SaveNewsItem(NewsItem item);
    }
}
