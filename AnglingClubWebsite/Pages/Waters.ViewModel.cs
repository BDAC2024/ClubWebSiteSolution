using AnglingClubShared.Entities;
using AnglingClubShared;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.RichTextEditor;
using System.Collections.ObjectModel;
using AnglingClubShared.DTOs;

namespace AnglingClubWebsite.Pages
{
    public partial class WatersViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWatersService _watersService;
        private readonly ILogger<WatersViewModel> _logger;

        public WatersViewModel(
            IMessenger messenger,
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IWatersService watersService,
            ILogger<WatersViewModel> logger) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
            _watersService = watersService;
            _logger = logger;
        }

        [ObservableProperty]
        private bool isUnlocked = false;

        [ObservableProperty]
        private ObservableCollection<WaterOutputDto> items = new ObservableCollection<WaterOutputDto>();

        [ObservableProperty]
        private WaterOutputDto? _water = null;

        [ObservableProperty]
        private bool _loading = false;

        [ObservableProperty]
        private bool _submitting = false;

        [ObservableProperty]
        private bool _isLoggedIn = false;

        public override async Task Loaded()
        {
            await getWaters();
            IsUnlocked = false;
            IsLoggedIn = await _authenticationService.isLoggedIn();
            await base.Loaded();
        }

        public void Unlock(bool unlock)
        {
            IsUnlocked = unlock;
        }

        private async Task getWaters(bool unlockAfterwards = false)
        {
            _messenger.Send(new ShowProgress());
            Loading = true;

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

                Loading = false;

                _messenger.Send(new HideProgress());
            }
        }

        public async Task OnWaterEdited(string itemId)
        {
            Water = Items.FirstOrDefault(i => i.DbKey == itemId);
            await Task.Delay(0);
        }

        private class What3Words
        {
            public string CarPark { get; set; } = "";
            public string Url { get; set; } = "";
        }
    }
}
