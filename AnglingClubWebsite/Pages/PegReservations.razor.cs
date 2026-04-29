using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class PegReservations : RazorComponentBase
    {
        private readonly IMessenger _messenger;
        private readonly ILogger<PegReservations> _logger;
        private readonly IPegReservationService _pegReservationService;
        private readonly IClubEventService _clubEventService;

        public PegReservations(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<PegReservations> logger,
            IPegReservationService pegReservationService,
            IClubEventService clubEventService) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _logger = logger;
            _pegReservationService = pegReservationService;
            _clubEventService = clubEventService;
        }

        public int SelectedTab { get; set; } = 0;
        public bool DataLoaded { get; set; } = false;
        public bool Registered { get; set; } = true;
        public bool RegisterMe { get; set; } = false;
        public bool RegistrationComplete { get; set; } = false;


        public PegRegistrationOutputDto? ExistingRegistration { get; set; } = null;
        public DateTime PresentationNightDate { get; set; } = DateTime.MinValue;

        public override async Task Loaded()
        {
            await RefreshAsync();
            await base.Loaded();
        }

        private async Task RefreshAsync()
        {
            DataLoaded = false;
            try
            {
                ExistingRegistration = await _pegReservationService.ReadRegistration(new PegRegistrationRequestDto
                {
                    Stretch = "MilbyIsland",
                    Peg = "1",
                    Season = EnumUtils.CurrentSeason()
                });

                var PresentationNight = (await _clubEventService.GetPresentationNightForSeason(EnumUtils.CurrentSeason())).FirstOrDefault();
                if (PresentationNight != null)
                {
                    PresentationNightDate = PresentationNight.Date;
                }
            }
            catch (ApiForbiddenException ex)
            {
                //                Message = "You are not authorised to use this page";
            }
            finally
            {
                DataLoaded = true;
                StateHasChanged();
            }

        }
        public void OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;
        }

        public async Task OnRegistering(Microsoft.AspNetCore.Components.ChangeEventArgs args)
        {
            Registered = false;

            try
            {
                await _pegReservationService.RegisterPeg(new PegRegistrationRequestDto
                {
                    Stretch = "MilbyIsland",
                    Peg = "1",
                    Season = EnumUtils.CurrentSeason()
                });
                RegistrationComplete = true;
            }
            catch (ApiForbiddenException ex)
            {
                //                Message = "You are not authorised to use this page";
            }
            finally
            {
                Registered = true;
                StateHasChanged();
            }

        }

    }
}
