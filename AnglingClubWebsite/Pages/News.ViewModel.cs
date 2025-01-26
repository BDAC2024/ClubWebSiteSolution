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
            IMessenger messenger,
            ICurrentUserService currentUserService) : base(messenger, currentUserService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }
    }
}
