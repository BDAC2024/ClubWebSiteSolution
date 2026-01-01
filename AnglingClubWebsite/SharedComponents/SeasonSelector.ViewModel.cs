using AnglingClubShared;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class SeasonSelectorViewModel : ViewModelBase, IRecipient<BrowserChange>
    {
        private readonly IMessenger _messenger;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentUserService _currentUserService;

        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;

        private readonly BrowserService _browserService;

        public SeasonSelectorViewModel(
            IMessenger messenger,
            IAuthenticationService authenticationService,
            ICurrentUserService currentUserService,
            IRefDataService refDataService,
            IGlobalService globalService,
            BrowserService browserService) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _refDataService = refDataService;
            _globalService = globalService;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);

            setBrowserDetails();
        }

        #region Properties

        [ObservableProperty]
        private ReferenceData? _refData;

        [ObservableProperty]
        private Season? _selectedSeason = Season.S20To21;

        [ObservableProperty]
        private bool _refDataLoaded = false;

        [ObservableProperty]
        private DeviceSize _browserSize = DeviceSize.Unknown;

        #endregion Properties

        #region Message Handlers

        public void Receive(BrowserChange message)
        {
            setBrowserDetails();
        }

        #endregion Message Handlers
        #region Methods

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

        private void setBrowserDetails()
        {
            BrowserSize = _browserService.DeviceSize;
        }

        #endregion Methods

        #region Events

        partial void OnSelectedSeasonChanged(Season? oldValue, Season? newValue)
        {
            if (oldValue != newValue && newValue != null)
            {
                _globalService.SetStoredSeason(newValue!.Value);

                OnSeasonChanged!.Invoke(newValue!.Value);
            }
        }

        #endregion Events

        #region Inter-component Events

        public Action<Season>? OnSeasonChanged { get; set; }

        #endregion Inter-component Events
    }
}
