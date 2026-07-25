using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class Index : RazorComponentBase
    {
        public Index(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IMessenger messenger) : base(messenger, currentUserService, authenticationService)
        {
        }
    }
}
