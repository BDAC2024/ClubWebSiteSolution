using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Blazor.Navigations;

namespace AnglingClubWebsite.Pages
{
    public partial class PegReservations : RazorComponentBase, IRecipient<LoggedIn>, IDisposable
    {
        private readonly IMessenger _messenger;
        private readonly ILogger<PegReservations> _logger;
        private readonly IPegReservationService _pegReservationService;
        private readonly IClubEventService _clubEventService;
        private readonly ICurrentUserService _currentUserService;

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
            _currentUserService = currentUserService;
        }

        public Season SelectedSeason { get; set; }
        public int SelectedTab { get; set; } = 0;
        public bool DataLoaded { get; set; } = false;
        public bool AllocationsLoaded { get; set; } = false;
        public bool Registered { get; set; } = true;
        public bool RegisterMe { get; set; } = false;
        public bool RegistrationComplete { get; set; } = false;

        public PegRegistrationOutputDto? ExistingRegistration { get; set; } = null;
        public DateTime PresentationNightDate { get; set; } = DateTime.MinValue;
        public DateOnly ReservationStartDate { get; set; }
        public List<PegAllocationOutputDto> PegAllocations { get; set; } = new List<PegAllocationOutputDto>();
        public IQueryable<PegAllocationOutputDto> QueryablePegAllocations { get; set; } = Enumerable.Empty<PegAllocationOutputDto>().AsQueryable();
        public IQueryable<PegAllocationOutputDto> QueryableExistingPegAllocations { get; set; }
        public List<PegRegistrationOutputDto> CurrentPegRegistrations { get; set; } = new List<PegRegistrationOutputDto>();
        public IQueryable<PegRegistrationOutputDto> QueryablePegRegistrations { get; set; } = new List<PegRegistrationOutputDto>().AsQueryable();

        public List<Member> EligibleMembers { get; set; } = new List<Member>();
        public int OtherMembershipNumber { get; set; }
        public bool RegisteredOther { get; set; } = true;
        public bool RegisteredOtherComplete { get; set; } = false;

        private bool _isLoggingIn = false;

        protected override async Task OnInitializedAsync()
        {
            _messenger.Register<LoggedIn>(this);

            await base.OnInitializedAsync();
        }

        public void Receive(LoggedIn message)
        {
            _currentUserService.User = message.User;
            CurrentUser = message.User;
            //Console.WriteLine("LOGIN RECEIVED");

            // Guard against concurrent/duplicate refresh calls
            if (_isLoggingIn)
            {
                //Console.WriteLine("LOGIN IGNORED - ALREADY DONE");
                return;
            }

            //Console.WriteLine("LOGIN PROCESSED");

            _isLoggingIn = true;
            _ = RefreshAsync();

        }

        public void Dispose()
        {
            _messenger.Unregister<LoggedIn>(this);
        }

        public override async Task Loaded()
        {
            await RefreshAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        private async Task RefreshAsync()
        {
            DataLoaded = false;

            SelectedSeason = EnumUtils.CurrentSeason();

            try
            {
                ExistingRegistration = await _pegReservationService.ReadRegistration(new PegRegistrationRequestDto
                {
                    Stretch = "MilbyIsland",
                    Peg = "1",
                    Season = SelectedSeason
                });

                var PresentationNight = (await _clubEventService.GetPresentationNightForSeason(SelectedSeason))!.FirstOrDefault();
                if (PresentationNight != null)
                {
                    PresentationNightDate = PresentationNight.Date;
                }
            }
            catch (ApiForbiddenException ex)
            {
                //                Message = "You are not authorised to use this page";
            }
            catch (Exception ex)
            {
                _logger.LogError($"RefreshAsync: {ex.Message}");
            }
            finally
            {
                DataLoaded = true;
                StateHasChanged();
            }
        }
        public async Task OnTabSelected(SelectEventArgs args)
        {
            SelectedTab = args.SelectedIndex;

            switch (SelectedTab)
            {
                case 1: // Always show an up to date list of allocations
                    await GetAllocations();
                    break;

                case 2: // Always show an up to date list of eligible members
                    RegisteredOtherComplete = false;
                    await GetEligibleMembers();
                    break;

                case 3: // Always show an up to date list of registrations
                    await GetRegistrations();
                    break;

                case 4: // Always show an up to date list of registrations
                    await SetupAllocator();
                    break;

                default:
                    break;
            }

        }

        public bool IsAdmin()
        {
            return _currentUserService.User.Admin;
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
                    Season = SelectedSeason
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

        private async Task OnAllocationChanged(int membershipNumber, PegAllocationOutputDto alloc)
        {
            if (membershipNumber != 0)
            {
                alloc.MembershipNumber = membershipNumber;
                {
                    var id = await _pegReservationService.AllocatePeg(new PegAllocationRequestDto
                    {
                        DateAllocated = alloc.DateAllocated,
                        MembershipNumber = alloc.MembershipNumber,
                        Peg = alloc.Peg,
                        Stretch = alloc.Stretch
                    });

                    alloc.DbKey = id;

                    return;
                }
            }
        }

        public async Task DeleteAllocation(PegAllocationOutputDto alloc)
        {
            await _pegReservationService.DeleteAllocatedPeg(alloc.DbKey);

            alloc.DbKey = "";
            alloc.MembershipNumber = 0;
            alloc.Name = null;
            StateHasChanged();
        }

        private async Task RegistraterOtherMember()
        {
            if (OtherMembershipNumber != 0)
            {
                {
                    RegisteredOther = false;
                    await _pegReservationService.RegisterOthersPeg(OtherMembershipNumber, new PegRegistrationRequestDto
                    {
                        Stretch = "MilbyIsland",
                        Peg = "1",
                        Season = SelectedSeason
                    });
                    RegisteredOther = true;
                    RegisteredOtherComplete = true;
                    await GetEligibleMembers();
                    return;
                }
            }
        }

        private async Task GetEligibleMembers()
        {
            var members = await _pegReservationService.ReadEligibleMembers(SelectedSeason);
            EligibleMembers = new List<Member>();
            EligibleMembers.Add(new Member
            {
                MembershipNumber = 0,
                Name = ""
            });
            EligibleMembers.AddRange(members);
            OtherMembershipNumber = EligibleMembers.First().MembershipNumber;
            StateHasChanged();

        }

        private async Task GetAllocations()
        {
            AllocationsLoaded = false;

            var existingAllocations =
                    await _pegReservationService.ReadAllocations(SelectedSeason)
                    ?? [];

            QueryableExistingPegAllocations = existingAllocations.AsQueryable();

            AllocationsLoaded = true;
        }

        private async Task GetRegistrations()
        {
            CurrentPegRegistrations = (await _pegReservationService.ReadRegistrations(SelectedSeason))!.OrderBy(x => x.Name).ToList() ?? new List<PegRegistrationOutputDto>();
            QueryablePegRegistrations = CurrentPegRegistrations.OrderBy(x => x.DateRegistered).AsQueryable();

        }

        private async Task SetupAllocator()
        {
            if (IsAdmin())
            {
                PegAllocations = new List<PegAllocationOutputDto>();

                await GetRegistrations();
                var ExistingAllocations = await _pegReservationService.ReadAllocations(SelectedSeason);

                ReservationStartDate = DateOnly.FromDateTime(new DateTime(SelectedSeason.SeasonStarts().Year, 6, 16)); ;
                var reservationEndDate = DateOnly.FromDateTime(new DateTime(ReservationStartDate.Year, 7, 31));

                var d = ReservationStartDate;
                while (d <= reservationEndDate)
                {
                    var existingAlloc = ExistingAllocations.FirstOrDefault(x => x.DateAllocated == d);
                    if (existingAlloc != null)
                    {
                        PegAllocations.Add(existingAlloc);
                    }
                    else
                    {
                        PegAllocations.Add(new PegAllocationOutputDto
                        {
                            Stretch = "MilbyIsland",
                            Peg = "1",
                            DateAllocated = d,
                            MembershipNumber = 0
                        });
                    }
                    d = d.AddDays(1);
                }

                PegAllocations = PegAllocations.OrderBy(x => x.DateAllocated).ToList();
                QueryablePegAllocations = PegAllocations.AsQueryable();

            }

        }
    }
}
