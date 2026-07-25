using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents.OnlyNeededWhilstMigrating;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class AppLink : ComponentBase
    {
        private readonly IConfiguration _configuration;
        private readonly HostBridge _hostBridge;
        private readonly IJSRuntime _jsRuntime;
        private readonly IMessenger _messenger;

        private bool _isEmbedded;

        public AppLink(
            IConfiguration configuration,
            HostBridge hostBridge,
            IJSRuntime jsRuntime,
            IMessenger messenger)
        {
            _configuration = configuration;
            _hostBridge = hostBridge;
            _jsRuntime = jsRuntime;
            _messenger = messenger;
        }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string Href { get; set; } = "/";

        // TODO Ang to Blazor Migration - only required until migration complete
        protected override async Task OnInitializedAsync()
        {
            _isEmbedded = await _hostBridge.IsEmbeddedAsync();
        }

        private async Task AppRedirect()
        {
            // TODO Ang to Blazor Migration - only required until migration complete
            if (_isEmbedded)
            {
                await _jsRuntime.InvokeVoidAsync("blazorHostBridge.requestAngPage", Href);
            }
            else
            {
                _messenger.Send(new SelectMenuItem(_configuration["BaseHref"] + Href)); // Just retain this line when migration complete
            }
        }
    }
}
