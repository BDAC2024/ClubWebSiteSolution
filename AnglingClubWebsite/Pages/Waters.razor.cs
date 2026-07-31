using AnglingClubShared.DTOs;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.RichTextEditor;

namespace AnglingClubWebsite.Pages
{
    public partial class Waters : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWatersService _watersService;
        private readonly ILogger<Waters> _logger;
        private readonly BrowserService _browserService;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _navigationManager;

        private bool _mapsNeedInitialization;
        private SfRichTextEditor _rteObjDesc = default!;
        private SfRichTextEditor _rteObjDirections = default!;

        public Waters(
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IWatersService watersService,
            ILogger<Waters> logger,
            BrowserService browserService,
            IJSRuntime js,
            NavigationManager navigationManager) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _authenticationService = authenticationService;
            _watersService = watersService;
            _logger = logger;
            _browserService = browserService;
            _js = js;
            _navigationManager = navigationManager;

            messenger.Register<BrowserChange>(this);
        }

        public bool IsUnlocked { get; set; }
        public List<WaterOutputDto> Items { get; set; } = new();
        public WaterOutputDto? Water { get; set; }
        public bool DataLoaded { get; set; }
        public bool Submitting { get; set; }
        public bool IsLoggedIn { get; set; }
        public double VideoWidth { get; set; } = 500;
        public double VideoHeight { get; set; } = 315;

        private List<ToolbarItemModel> Tools { get; } = new()
        {
            new ToolbarItemModel() { Command = ToolbarCommand.Bold },
            new ToolbarItemModel() { Command = ToolbarCommand.Italic },
            new ToolbarItemModel() { Command = ToolbarCommand.Formats },
            new ToolbarItemModel() { Command = ToolbarCommand.FontName },
            new ToolbarItemModel() { Command = ToolbarCommand.FontSize },
            new ToolbarItemModel() { Command = ToolbarCommand.FontColor },
            new ToolbarItemModel() { Command = ToolbarCommand.BackgroundColor },
            new ToolbarItemModel() { Command = ToolbarCommand.Alignments },
            new ToolbarItemModel() { Command = ToolbarCommand.NumberFormatList },
            new ToolbarItemModel() { Command = ToolbarCommand.BulletFormatList },
            new ToolbarItemModel() { Command = ToolbarCommand.Indent },
            new ToolbarItemModel() { Command = ToolbarCommand.Outdent },
            new ToolbarItemModel() { Command = ToolbarCommand.Undo },
            new ToolbarItemModel() { Command = ToolbarCommand.Redo }
        };

        public void Receive(BrowserChange message)
        {
            SetVideoSize();
            _ = InvokeAsync(StateHasChanged);
        }

        public override async Task Loaded()
        {
            SetVideoSize();
            await GetWaters();
            IsUnlocked = false;
            IsLoggedIn = await _authenticationService.isLoggedIn();

            await base.Loaded();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_mapsNeedInitialization)
            {
                _mapsNeedInitialization = false;

                foreach (var item in Items)
                {
                    await _js.InvokeVoidAsync("initializeMaps", $"map-{item.DbKey}", item.Centre, item.Markers.ToArray(), item.Path.ToArray());
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public string DirectionUrl(WaterOutputDto water)
        {
            return $"{Constants.MAP_DIRECTIONS_BASE_URL}/{water.Destination.Lat},{water.Destination.Long}";
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        public async Task OnWaterEdited(string itemId)
        {
            Water = Items.FirstOrDefault(item => item.DbKey == itemId);
            await Task.CompletedTask;
        }

        private async Task Cancel()
        {
            Water = null;
            await GetWaters(true);
            await InvokeAsync(StateHasChanged);
        }

        private async Task Save()
        {
            DataLoaded = false;

            try
            {
                Submitting = true;
                await _watersService.SaveWater(Water!);
                await GetWaters(true);
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowMessage(MessageState.Error, "Save Failed", "Unable to save Water"));
                _logger.LogError(ex, "Failed to save water");
            }
            finally
            {
                Submitting = false;
                Water = null;
                DataLoaded = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task GetWaters(bool unlockAfterwards = false)
        {
            DataLoaded = false;

            try
            {
                var items = await _watersService.ReadWaters();

                if (items is not null)
                {
                    Items = items.ToList();
                    _mapsNeedInitialization = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load waters");
            }
            finally
            {
                if (unlockAfterwards)
                {
                    Unlock(false);
                    Unlock(true);
                }

                DataLoaded = true;
            }
        }

        private void SetVideoSize()
        {
            VideoWidth = _browserService.IsPortrait ? 260 : 500;
            VideoHeight = VideoWidth / (16.0 / 9.0);

            _messenger.Send(new ShowConsoleMessage($"Portrait: {_browserService.IsPortrait}, Width: {VideoWidth}, Height: {VideoHeight}"));
        }

        private async Task LoginThenRedirect()
        {
            await _js.InvokeVoidAsync("blazorHostBridge.requestLogin");
        }

        private bool OnLocalhost()
        {
            return _navigationManager.Uri.Contains("localhost");
        }

        private bool ItemBeingEdited(WaterOutputDto water)
        {
            return Water is not null && Water.DbKey == water.DbKey;
        }

        private void ToolbarClick(ToolbarClickEventArgs args)
        {
            _rteObjDesc.PreventRender();
            _rteObjDirections.PreventRender();
        }
    }
}
