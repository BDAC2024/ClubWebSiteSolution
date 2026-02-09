using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;

namespace AnglingClubWebsite.SharedComponents
{
    public partial class SeasonSelector : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessenger _messenger;
        private readonly ICurrentUserService _currentUserService;

        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;
        private readonly ILogger<SeasonSelector> _logger;

        private readonly BrowserService _browserService;

        public SeasonSelector(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<SeasonSelector> logger,
            IRefDataService refDataService,
            IGlobalService globalService,
            BrowserService browserService) : base(messenger, currentUserService, authenticationService)
        {
            messenger.Register<BrowserChange>(this);

            _authenticationService = authenticationService;
            _messenger = messenger;
            _currentUserService = currentUserService;
            _logger = logger;
            _refDataService = refDataService;
            _globalService = globalService;
            _browserService = browserService;

            setBrowserDetails();
        }

        [Parameter]
        public EventCallback<Season?> SelectedSeasonChanged { get; set; }

        // Host "method" to call whenever it changes
        [Parameter]
        public EventCallback<Season?> OnSeasonChanged { get; set; }

        #region Properties

        public ReferenceData? RefData;

        public Season? SelectedSeason = Season.S20To21;

        public bool RefDataLoaded = false;

        public DeviceSize BrowserSize = DeviceSize.Unknown;

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
                _logger.LogError($"getRefData: {ex.Message}");
            }
            finally
            {
                RefDataLoaded = true;
            }
        }

        #endregion Methods

        #region Events

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        private async Task OnValueChanged(Season? newSeason)
        {
            // 1) update our own state
            SelectedSeason = newSeason;
            _globalService.SetStoredSeason(SelectedSeason!.Value);

            // 2) support bind-SelectedSeason on the host (optional but recommended)
            if (SelectedSeasonChanged.HasDelegate)
            {
                await SelectedSeasonChanged.InvokeAsync(newSeason);
            }

            // 3) notify the host explicitly (your requirement)
            if (OnSeasonChanged.HasDelegate)
            {
                await OnSeasonChanged.InvokeAsync(newSeason);
            }
        }

        #endregion Events

        #region Helper Methods

        private void setBrowserDetails()
        {
            BrowserSize = _browserService.DeviceSize;
        }

        #endregion Helper Methods
    }

}
