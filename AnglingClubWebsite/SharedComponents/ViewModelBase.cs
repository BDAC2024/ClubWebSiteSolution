using AnglingClubShared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents
{
    public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
    {
        private readonly IMessenger _messenger;

        protected ViewModelBase(IMessenger messenger)
        {
            _messenger = messenger;
        }

        protected virtual void NotifyStatChanged() => OnPropertyChanged((string?)null);

        public virtual async Task OnInitializedAsync()
        {
            await Loaded().ConfigureAwait(true);
        }

        [RelayCommand]
        public virtual async Task Loaded()
        {

            await Task.CompletedTask.ConfigureAwait(false);
        }

    }

}
