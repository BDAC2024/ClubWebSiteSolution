using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.Dialogs
{
    public partial class AddMinutes
    {
        [Parameter] required public bool Visible { get; set; } = false;
        /// <summary>
        /// This is a name-based convention that will trigger a 2-way binding.
        /// The caller does not need to set register for the callback, blazor
        /// will handle it as long as the Visible property is bound with 
        /// @bind-Visible="ShowingResults" rather than the 1-way method
        /// of setting Value="ShowingResults"
        /// </summary>
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }


        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;

        public AddMinutes(
            ICurrentUserService currentUserService, 
            IAuthenticationService authenticationService, 
            IMessenger messenger) : base (messenger, currentUserService, authenticationService)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

        private async Task CloseAsync()
        {
            // Tell the parent to update its source of truth
            await VisibleChanged.InvokeAsync(false);
        }
    }
}
