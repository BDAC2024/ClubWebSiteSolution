namespace AnglingClubWebsite.Services
{
    public interface INavigationService
    {
        Task GoBack();
        void NavigateTo(string route, bool forceLoad = false);
    }
}
