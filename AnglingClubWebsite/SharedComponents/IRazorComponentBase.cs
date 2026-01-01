using System.ComponentModel;

namespace AnglingClubWebsite.SharedComponents
{
    public interface IRazorComponentBase
    {
        Task OnInitializedAsync();
        Task Loaded();
    }
}
