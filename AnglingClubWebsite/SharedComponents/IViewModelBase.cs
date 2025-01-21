using System.ComponentModel;

namespace AnglingClubWebsite.SharedComponents
{
    public interface IViewModelBase : INotifyPropertyChanged
    {
        Task OnInitializedAsync();
        Task Loaded();
    }
}
