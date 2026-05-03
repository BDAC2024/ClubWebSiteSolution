using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IPegReservationService
    {
        Task<List<PegRegistrationOutputDto>?> ReadRegistrations(Season season);
        Task<PegRegistrationOutputDto?> ReadRegistration(PegRegistrationRequestDto registration);
        Task RegisterPeg(PegRegistrationRequestDto registration);
        Task RegisterOthersPeg(int membershipNumber, PegRegistrationRequestDto registration);
        Task<List<Member>> ReadEligibleMembers(Season season);
        Task<List<PegAllocationOutputDto>> ReadAllocations(Season season);
        Task<string> AllocatePeg(PegAllocationRequestDto allocation);
        Task DeleteAllocatedPeg(string id);
    }
}