using AnglingClubShared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface INewsRepository
    {
        Task AddOrUpdateNewsItem(NewsItem newsItem);
        Task<List<NewsItem>> GetNewsItems();
        Task DeleteNewsItem(string id);
    }
}