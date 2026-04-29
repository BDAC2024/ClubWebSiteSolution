using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Services;
using AnglingClubWebsite.SharedComponents;
using CommunityToolkit.Mvvm.Messaging;

namespace AnglingClubWebsite.Pages
{
    public partial class PegAllocator : RazorComponentBase
    {
        private readonly IMessenger _messenger;
        private readonly ILogger<PegAllocator> _logger;
        private readonly IPegReservationService _pegReservationService;
        private readonly IClubEventService _clubEventService;

        public PegAllocator(
            IAuthenticationService authenticationService,
            IMessenger messenger,
            ICurrentUserService currentUserService,
            ILogger<PegAllocator> logger,
            IPegReservationService pegReservationService,
            IClubEventService clubEventService) : base(messenger, currentUserService, authenticationService)
        {
            _messenger = messenger;
            _logger = logger;
            _pegReservationService = pegReservationService;
            _clubEventService = clubEventService;
        }

        public bool DataLoaded { get; set; } = false;


        public List<PegAllocationOutputDto> PegAllocations { get; set; } = new List<PegAllocationOutputDto>();
        public IQueryable<PegAllocationOutputDto> QueryablePegAllocations { get; set; }
        public List<PegRegistrationOutputDto> PegRegistrations { get; set; } = new List<PegRegistrationOutputDto>();

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
                var ExistingAllocations = await _pegReservationService.ReadAllocations(EnumUtils.CurrentSeason());

                var reservationStartDate = DateOnly.FromDateTime(new DateTime((EnumUtils.CurrentSeason()).SeasonStarts().Year, 6, 16)); ;
                var reservationEndDate = DateOnly.FromDateTime(new DateTime(reservationStartDate.Year, 7, 31));

                var d = reservationStartDate;
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

                PegRegistrations = (await _pegReservationService.ReadRegistrations(EnumUtils.CurrentSeason())) ?? new List<PegRegistrationOutputDto>();
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

        private async Task OnAllocationChanged(int membershipNumber, PegAllocationOutputDto alloc)
        {
            alloc.MembershipNumber = membershipNumber;

            await _pegReservationService.AllocatePeg(new PegAllocationRequestDto
            {
                DateAllocated = alloc.DateAllocated,
                MembershipNumber = alloc.MembershipNumber,
                Peg = alloc.Peg,
                Stretch = alloc.Stretch
            });
        }
    }
}
