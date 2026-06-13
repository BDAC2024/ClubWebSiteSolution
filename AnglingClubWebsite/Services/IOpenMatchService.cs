using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IOpenMatchService
    {
        Task<List<OpenMatchDto>> ReadMatches(Season season);
        Task<OpenMatchRegistrationDto?> SubmitRegistration(OpenMatchRegistrationDto registration);
        Task DeleteRegistration(string id);
        Task<List<OpenMatchRegistrationDto>> ReadRegistrations(Season season);
    }
}
