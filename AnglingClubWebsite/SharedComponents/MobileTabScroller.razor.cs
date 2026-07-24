using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class MobileTabScroller
    {

        private ElementReference _container;
        private IJSObjectReference? _module;
        private IJSObjectReference? _instance;
        private bool _initialised;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string? CssClass { get; set; }

        [Parameter]
        public double ScrollAmount { get; set; } = 180;

        [Parameter]
        public string LeftArrowLabel { get; set; } =
            "Show previous tabs";

        [Parameter]
        public string RightArrowLabel { get; set; } =
            "Show more tabs";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_initialised)
            {
                return;
            }

            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import",
                "./mobileTabScroller.js");

            _instance = await _module.InvokeAsync<IJSObjectReference>(
                "initialise",
                _container);

            _initialised = true;
        }

        private async Task ScrollLeftAsync()
        {
            if (_instance is null)
            {
                return;
            }

            await _instance.InvokeVoidAsync(
                "scroll",
                -ScrollAmount);
        }

        private async Task ScrollRightAsync()
        {
            if (_instance is null)
            {
                return;
            }

            await _instance.InvokeVoidAsync(
                "scroll",
                ScrollAmount);
        }

        public async ValueTask DisposeAsync()
        {
            if (_instance is not null)
            {
                try
                {
                    await _instance.InvokeVoidAsync("dispose");
                    await _instance.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // The browser session has already disconnected.
                }
            }

            if (_module is not null)
            {
                try
                {
                    await _module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // The browser session has already disconnected.
                }
            }
        }

    }
}