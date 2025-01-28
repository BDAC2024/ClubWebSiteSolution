using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents
{
    public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        protected ViewModelBase(
            IMessenger messenger, 
            ICurrentUserService currentUserService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
        }

        [ObservableProperty]
        private MemberDto? _currentUser;

        protected virtual void NotifyStatChanged() => OnPropertyChanged((string?)null);

        public virtual async Task OnInitializedAsync()
        {
            await Loaded().ConfigureAwait(true);
        }

        [RelayCommand]
        public virtual async Task Loaded()
        {

            await Task.CompletedTask.ConfigureAwait(false);

            CurrentUser = _currentUserService.User;
        }

        public void NavToPage(string page)
        {
            _messenger.Send<SelectMenuItem>(new SelectMenuItem(page));
        }
    }

}
