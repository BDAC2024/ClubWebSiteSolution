using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AnglingClubWebsite.Services
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public NavigationService(NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        public async Task GoBack()
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }

        public void NavigateTo(string route, bool forceLoad = false)
        {
            _navigationManager.NavigateTo(route ?? "/", forceLoad);
        }
    }
}
