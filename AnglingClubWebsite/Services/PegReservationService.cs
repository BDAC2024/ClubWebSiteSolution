using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubWebsite.Helpers;
using AnglingClubWebsite.Models;
using AutoMapper;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class PegReservationService : DataServiceBase, IPegReservationService
    {
        private const string CONTROLLER = "Waters";

        private readonly ILogger<PegReservationService> _logger;
        private readonly IMessenger _messenger;
        private readonly object _authenticationService;
        private readonly object _mapper;

        public PegReservationService(
            IHttpClientFactory httpClientFactory,
            ILogger<PegReservationService> logger,
            IMessenger messenger,
            IAuthenticationService authenticationService,
            IMapper mapper) : base(httpClientFactory)
        {
            _logger = logger;
            _messenger = messenger;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }

        public async Task<List<PegRegistrationOutputDto>?> ReadRegistrations(Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATIONS_READ}?Season={(int)season}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<PegRegistrationOutputDto>>();

            return content;
        }

        public async Task<PegRegistrationOutputDto?> ReadRegistration(PegRegistrationRequestDto registration)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_READ}";

            try
            {
                var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", registration);
                return await response.Content.ReadFromJsonAsync<PegRegistrationOutputDto>();
            }
            catch (ApiNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ReadRegistration: {ex.Message}");
                return null;
            }

        }

        public async Task RegisterPeg(PegRegistrationRequestDto registration)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_REGISTER_PEG}";

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", registration);

            return;

        }

        /// <summary>
        /// Admin only - register a peg on behalf of another member
        /// </summary>
        /// <param name="membershipNumber"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task RegisterOthersPeg(int membershipNumber, PegRegistrationRequestDto registration)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_REGISTER_OTHERS_PEG}/{membershipNumber}";

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", registration);

            return;

        }

        /// <summary>
        /// Get a list of members that havent yey registered for a peg in the specified season;
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        public async Task<List<Member>> ReadEligibleMembers(Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_READ_ELIGIBLE_MEMBERS}?Season={(int)season}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<Member>>();

            return content ?? new List<Member>();
        }

        public async Task<List<PegAllocationOutputDto>> ReadAllocations(Season season)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_PEG_ALLOCATIONS}?Season={(int)season}";

            var response = await Http.GetAsync($"{relativeEndpoint}");

            var content = await response.Content.ReadFromJsonAsync<List<PegAllocationOutputDto>>();

            return content ?? new List<PegAllocationOutputDto>();
        }

        public async Task<string> AllocatePeg(PegAllocationRequestDto allocation)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_ALLOCATE_PEG}";

            var response = await Http.PostAsJsonAsync($"{relativeEndpoint}", allocation);

            var content = await response.Content.ReadAsStringAsync();

            return content ?? "";

        }

        public async Task DeleteAllocatedPeg(string id)
        {
            var relativeEndpoint = $"{CONTROLLER}/{Constants.API_REGISTRATION_PEG_ALLOCATIONS}/{id}";

            var response = await Http.DeleteAsync($"{relativeEndpoint}");

            return;

        }

    }
}
