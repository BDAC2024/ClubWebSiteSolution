using AnglingClubShared;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class SeasonSelectorViewModel : ViewModelBase
    {
        private readonly IMessenger _messsenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;

        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;

        public SeasonSelectorViewModel(
            IMessenger messsenger,
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IRefDataService refDataService,
            IGlobalService globalService) : base(messsenger, currentUserService, authenticationService)
        {
            _messsenger = messsenger;
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _refDataService = refDataService;
            _globalService = globalService;
        }

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private Season? _selectedSeason = Season.S20To21;

        [ObservableProperty]
        private bool _refDataLoaded = false;

        public override async Task Loaded()
        {
            await getRefData();
            await base.Loaded();
        }

        private async Task getRefData()
        {
            try
            {
                RefData = await _refDataService.ReadReferenceData();
                SelectedSeason = _globalService.GetStoredSeason(RefData!.CurrentSeason);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"getRefData: {ex.Message}");
            }
            finally
            {
                RefDataLoaded = true;
            }
        }

        partial void OnSelectedSeasonChanged(Season? oldValue, Season? newValue)
        {
            if (oldValue != newValue && newValue != null)
            {
                //_logger.LogWarning($"Venue {newValue!.Value} selected in VM.OnSelectedItemIdChanged");
                //Console.WriteLine($"Venue {newValue!.Value} selected in VM.OnSelectedItemIdChanged");
                _globalService.SetStoredSeason(newValue!.Value);

                OnSeasonChanged!.Invoke(newValue!.Value);
            }
        }

        public Action<Season>? OnSeasonChanged { get; set; }
    }
}
