using AnglingClubShared.Entities;
using AnglingClubShared;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.RichTextEditor;
using System.Collections.ObjectModel;
using AnglingClubShared.DTOs;
using Microsoft.JSInterop;
using AnglingClubShared.Models;
using AnglingClubShared.Enums;
using CommunityToolkit.Mvvm.Input;
using Fishing.Client.Services;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AnglingClubWebsite.Pages
{
    public partial class WatersViewModel : ViewModelBase, IRecipient<BrowserChange>
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWatersService _watersService;
        private readonly ILogger<WatersViewModel> _logger;
        private readonly BrowserService _browserService;
        private readonly IJSRuntime _js;
        private readonly IAppDialogService _appDialogService;

        public WatersViewModel(
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IWatersService watersService,
            ILogger<WatersViewModel> logger,
            BrowserService browserService,
            IJSRuntime js,
            IAppDialogService appDialogService) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
            _watersService = watersService;
            _logger = logger;

            messenger.Register<BrowserChange>(this);
            _browserService = browserService;
            _js = js;
            _appDialogService = appDialogService;
        }

        [ObservableProperty]
        private bool isUnlocked = false;

        [ObservableProperty]
        private ObservableCollection<WaterOutputDto> items = new ObservableCollection<WaterOutputDto>();

        [ObservableProperty]
        private WaterOutputDto? _water = null;

        [ObservableProperty]
        private bool _dataLoaded = false;

        [ObservableProperty]
        private bool _submitting = false;

        [ObservableProperty]
        private bool _isLoggedIn = false;

        [ObservableProperty]
        private double _videoWidth = 500;

        [ObservableProperty]
        private double _videoHeight = 315;

        #region Message Handlers

        public void Receive(BrowserChange message)
        {
            setVideoSize();
        }

        #endregion Message Handlers

        public override async Task Loaded()
        {
            setVideoSize();
            await getWaters();
            IsUnlocked = false;
            IsLoggedIn = await _authenticationService.isLoggedIn();

            foreach (var item in Items)
            {
                await _js.InvokeVoidAsync("initializeMaps", $"map-{item.DbKey}", item.Centre, item.Markers.ToArray(), item.Path.ToArray());
            }

            await base.Loaded();
        }

        private void setVideoSize()
        {
            VideoWidth = _browserService.IsPortrait ? 260 : 500;
            VideoHeight = VideoWidth / (16.0 / 9.0);

            _messenger.Send(new ShowConsoleMessage($"Portrait: {_browserService.IsPortrait}, Width: {VideoWidth}, Height: {VideoHeight}"));

        }

        public string DirectionUrl(WaterOutputDto water) 
        {
            return $"{Constants.MAP_DIRECTIONS_BASE_URL}/{water.Destination.Lat},{water.Destination.Long}";
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        private async Task getWaters(bool unlockAfterwards = false)
        {
            DataLoaded = false;

            try
            {
                var items = await _watersService.ReadWaters();

                if (items != null)
                {
                    Items = new ObservableCollection<WaterOutputDto>(items);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"getNews: {ex.Message}");
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

        public async Task OnWaterEdited(string itemId)
        {
            Water = Items.FirstOrDefault(i => i.DbKey == itemId);
            await Task.Delay(0);
        }

        [RelayCommand]
        private async Task Cancel()
        {
            Water = null;

            await getWaters(true);
            await Task.Delay(0);
        }

        [RelayCommand(CanExecute = nameof(CanWeSave))]
        private async Task Save()
        {
            DataLoaded = false;

            try
            {
                Submitting = true;
                await _watersService.SaveWater(Water!);
                await getWaters(true);
            }
            catch (Exception ex)
            {
                _appDialogService.SendMessage(MessageState.Error, "Save Failed", "Unable to save Water");
                _logger.LogError(ex, "Failed to save water");
            }
            finally
            {
                Submitting = false;
                Water = null;
                DataLoaded = true;
            }
        }

        public bool CanWeSave()
        {
            return true;
            //var valid = !(LoginModel.HasErrors || Submitting);
            //return valid;
        }

        private class What3Words
        {
            public string CarPark { get; set; } = "";
            public string Url { get; set; } = "";
        }
    }
}
