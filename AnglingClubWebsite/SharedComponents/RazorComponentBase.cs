using AnglingClubShared;
using AnglingClubShared.DTOs;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.SharedComponents
{
    public abstract partial class RazorComponentBase : ComponentBase, IRazorComponentBase
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;

        protected RazorComponentBase(
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
        }

        protected ClientMemberDto CurrentUser = new ClientMemberDto();

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
