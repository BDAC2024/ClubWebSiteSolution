using AnglingClubShared.DTOs;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class NewsViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public NewsViewModel(
            IAuthenticationService authenticationService,
            IMessenger messenger) : base(messenger)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

        [ObservableProperty]
        private MemberDto? _user;

        public override async Task Loaded()
        {
            await base.Loaded();
            var user = await _authenticationService.GetCurrentUser();
            if (user != null)
            {
                User = user;
            }

        }
    }
}
