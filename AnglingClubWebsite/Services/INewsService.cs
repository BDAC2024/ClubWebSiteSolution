using AnglingClubShared.Entities;

namespace AnglingClubWebsite.Services
{
    public interface INewsService
    {
        Task<List<NewsItem>?> ReadNews();
    }
}
