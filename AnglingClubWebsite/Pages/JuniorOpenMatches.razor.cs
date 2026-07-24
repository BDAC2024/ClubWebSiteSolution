using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Navigations;
using DialogSeverity = AnglingClubWebsite.Models.DialogSeverity;

namespace AnglingClubWebsite.Pages
{
    public partial class JuniorOpenMatches : RazorComponentBase, IRecipient<BrowserChange>
    {
        private readonly IOpenMatchService _openMatchService;
        private readonly IRefDataService _refDataService;
        private readonly IGlobalService _globalService;
        private readonly IDialogQueue _dialogQueue;
        private readonly ILogger<JuniorOpenMatches> _logger;
        private readonly BrowserService _browserService;

        public JuniorOpenMatches(
            ICurrentUserService currentUserService,
            IAuthenticationService authenticationService,
            IMessenger messenger,
            IOpenMatchService openMatchService,
            IRefDataService refDataService,
            IGlobalService globalService,
            IDialogQueue dialogQueue,
            BrowserService browserService,
            ILogger<JuniorOpenMatches> logger) : base(messenger, currentUserService, authenticationService)
        {
            _openMatchService = openMatchService;
            _refDataService = refDataService;
            _globalService = globalService;
            _dialogQueue = dialogQueue;
            _browserService = browserService;

            messenger.Register<BrowserChange>(this);
            BrowserSize = _browserService.DeviceSize;

            _logger = logger;
        }

        [Parameter]
        public int? Tab { get; set; }

        public bool DataLoaded { get; set; }
        public bool MatchesLoaded { get; set; }
        public bool RegistrationsLoaded { get; set; }
        public bool IsRegistering { get; set; }
        public bool IsListing { get; set; }
        public bool IsSubmitting { get; set; }
        public bool RegistrationSuccessful { get; set; }
        public bool IsAdmin => CurrentUser.Admin;
        public int SelectedTab { get; set; }
        public Season SelectedSeason { get; set; }
        public string Message { get; set; } = string.Empty;

        private DeviceSize BrowserSize = DeviceSize.Unknown;

        public List<OpenMatchDto> Matches { get; set; } = new();
        public IQueryable<OpenMatchDto>? MatchesQueryable;

        public List<OpenMatchRegistrationDto> SelectedRegistrations { get; set; } = new();
        public IQueryable<OpenMatchRegistrationDto>? SelectedRegistrationsQueryable;

        public OpenMatchDto? RegistrationMatch { get; set; }
        public OpenMatchRegistrationDto Registration { get; set; } = new();
        public OpenMatchRegistrationDto? SuccessfulRegistration { get; set; }

        public List<AgeGroupOption> AgeGroupOptions { get; } = new()
        {
            new AgeGroupOption { Value = JuniorAgeGroup.UpTo12, Text = "Up to 12 years" },
            new AgeGroupOption { Value = JuniorAgeGroup.ThirteenTo18, Text = "13 to 18 years" }
        };

        public string RegistrationSummary {
            get
            {
                var upTo12 = SelectedRegistrations.Count(x => x.AgeGroup == JuniorAgeGroup.UpTo12);
                var thirteenTo18 = SelectedRegistrations.Count(x => x.AgeGroup == JuniorAgeGroup.ThirteenTo18);
                var upTo12IsAre = upTo12 == 1 ? "is" : "are";
                var thirteenTo18IsAre = thirteenTo18 == 1 ? "is" : "are";

                return $"Currently <b>{SelectedRegistrations.Count}</b> registered; <b>{upTo12}</b> {upTo12IsAre} up to 12 and <b>{thirteenTo18}</b> {thirteenTo18IsAre} 13 to 18";
            }
        }

        public void Receive(BrowserChange message)
        {
            BrowserSize = _browserService.DeviceSize;
        }


        public override async Task Loaded()
        {
            await LoadInitialData();

            if (Tab.HasValue)
            {
                SelectedTab = Tab.Value;
                await LoadSelectedTabData();
            }

            await base.Loaded();
        }

        public async Task OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;

            await LoadSelectedTabData();
        }

        private async Task LoadSelectedTabData()
        {
            if (SelectedTab == 1 && !MatchesLoaded)
            {
                await LoadMatches();
            }
        }

        public async Task SeasonChanged(Season? season)
        {
            if (season is null)
            {
                return;
            }

            SelectedSeason = season.Value;
            _globalService.SetStoredSeason(SelectedSeason);
            await LoadMatches();
        }

        public async Task ShowRegistrationTab()
        {
            SelectedTab = 1;

            if (!MatchesLoaded)
            {
                await LoadMatches();
            }
        }

        public void StartRegistration(OpenMatchDto match)
        {
            RegistrationMatch = match;
            Registration = new OpenMatchRegistrationDto
            {
                OpenMatchId = match.DbKey
            };
            SuccessfulRegistration = null;
            RegistrationSuccessful = false;
            IsListing = false;
            IsRegistering = true;
            Message = string.Empty;
        }

        public void CancelRegistration()
        {
            IsRegistering = false;
            IsSubmitting = false;
            Registration = new OpenMatchRegistrationDto();
        }

        public async Task SubmitRegistration()
        {
            if (RegistrationMatch is null)
            {
                return;
            }

            IsSubmitting = true;
            Message = string.Empty;

            try
            {
                SuccessfulRegistration = await _openMatchService.SubmitRegistration(Registration);
                IsRegistering = false;
                await LoadMatches();
                RegistrationSuccessful = true;

                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Toast,
                    Severity = DialogSeverity.Success,
                    Message = "Registration submitted"
                });
            }
            catch (ApiValidationException ex)
            {
                Message = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit Junior Open Match registration");
                Message = "Registration failed. Please try again later.";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        public async Task ViewRegistrations(OpenMatchDto match)
        {
            RegistrationMatch = match;
            IsListing = true;
            IsRegistering = false;
            RegistrationSuccessful = false;
            await LoadRegistrations(match.DbKey);
        }

        public void ConfirmDeleteRegistration(OpenMatchRegistrationDto registration)
        {
            _dialogQueue.Enqueue(new DialogRequest
            {
                Kind = DialogKind.Confirm,
                Severity = DialogSeverity.Warn,
                Title = "Please Confirm",
                Message = $"Are you sure you want to delete the registration for {registration.Name}?",
                ConfirmText = "Delete",
                OnConfirmAsync = async () => await DeleteRegistration(registration)
            });
        }

        private async Task LoadInitialData()
        {
            DataLoaded = false;

            try
            {
                var refData = await _refDataService.ReadReferenceData();
                SelectedSeason = _globalService.GetStoredSeason(refData?.CurrentSeason ?? Season.S26To27);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Junior Open Match reference data");
                SelectedSeason = Season.S26To27;
            }
            finally
            {
                DataLoaded = true;
            }
        }

        private async Task LoadMatches()
        {
            MatchesLoaded = false;
            Message = string.Empty;
            IsRegistering = false;
            IsListing = false;
            RegistrationSuccessful = false;

            try
            {
                Matches = (await _openMatchService.ReadMatches(SelectedSeason))
                    .OrderBy(x => x.Date)
                    .ToList();
                MatchesQueryable = Matches.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Junior Open Matches for {Season}", SelectedSeason);
                Message = "Junior Open matches could not be loaded. Please try again later.";
                Matches = new List<OpenMatchDto>();
                MatchesQueryable = Matches.AsQueryable();
            }
            finally
            {
                MatchesLoaded = true;
                StateHasChanged();
            }
        }

        private async Task LoadRegistrations(string matchId)
        {
            RegistrationsLoaded = false;
            SelectedRegistrations = new List<OpenMatchRegistrationDto>();

            try
            {
                var registrations = await _openMatchService.ReadRegistrations(SelectedSeason);
                SelectedRegistrations = registrations
                    .Where(x => x.OpenMatchId == matchId)
                    .OrderBy(x => x.Name)
                    .Select(EnsureAgeGroupDescription)
                    .ToList();
                SelectedRegistrationsQueryable = SelectedRegistrations.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Junior Open Match registrations for {MatchId}", matchId);
                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Toast,
                    Severity = DialogSeverity.Error,
                    Message = "Registrations could not be loaded"
                });
            }
            finally
            {
                RegistrationsLoaded = true;
            }
        }

        private async Task DeleteRegistration(OpenMatchRegistrationDto registration)
        {
            try
            {
                await _openMatchService.DeleteRegistration(registration.DbKey);
                await LoadMatches();

                if (RegistrationMatch is not null)
                {
                    await ViewRegistrations(RegistrationMatch);
                }

                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Toast,
                    Severity = DialogSeverity.Success,
                    Message = "Registration deleted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Junior Open Match registration {RegistrationId}", registration.DbKey);
                _dialogQueue.Enqueue(new DialogRequest
                {
                    Kind = DialogKind.Toast,
                    Severity = DialogSeverity.Error,
                    Message = "Registration could not be deleted"
                });
            }
        }

        private OpenMatchRegistrationDto EnsureAgeGroupDescription(OpenMatchRegistrationDto registration)
        {
            if (string.IsNullOrWhiteSpace(registration.AgeGroupAsString) && registration.AgeGroup is not null)
            {
                registration.AgeGroupAsString = registration.AgeGroup.Value.EnumDescription();
            }

            return registration;
        }

        public string CellClass(OpenMatchDto row)
        {
            var classes = "bdac-rowcell";

            // Dim past rows
            if (row.InThePast)
            {
                classes += " bdac-row-past";
            }

            return classes;
        }

    }

    public class AgeGroupOption
    {
        public JuniorAgeGroup Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
