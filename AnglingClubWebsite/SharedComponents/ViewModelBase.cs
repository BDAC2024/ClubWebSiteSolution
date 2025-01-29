using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Fishing.Client.Services;

namespace AnglingClubWebsite.SharedComponents
{
    public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;

        protected ViewModelBase(
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
        }

        [ObservableProperty]
        private MemberDto _currentUser = new MemberDto();

        protected virtual void NotifyStatChanged() => OnPropertyChanged((string?)null);

        public virtual async Task OnInitializedAsync()
        {
            _currentUserService.User = await _authenticationService.GetCurrentUser();
            CurrentUser = _currentUserService.User;

            await Loaded().ConfigureAwait(true);
        }

        public virtual async Task Loaded()
        {

            await Task.CompletedTask.ConfigureAwait(false);

        }

        public void NavToPage(string page)
        {
            _messenger.Send<SelectMenuItem>(new SelectMenuItem(page));
        }
    }

}
