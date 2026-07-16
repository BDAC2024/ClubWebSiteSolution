using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class OpenMatchService : DataServiceBase, IOpenMatchService
    {
        private const string CONTROLLER = "OpenMatch";

        public OpenMatchService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<List<OpenMatchDto>> ReadMatches(Season season)
        {
            var response = await Http.GetAsync($"{CONTROLLER}/Matches/{(int)season}");
            return await response.Content.ReadFromJsonAsync<List<OpenMatchDto>>() ?? new List<OpenMatchDto>();
        }

        public async Task<OpenMatchRegistrationDto?> SubmitRegistration(OpenMatchRegistrationDto registration)
        {
            var response = await Http.PostAsJsonAsync($"{CONTROLLER}/MatchRegistration", registration);
            return await response.Content.ReadFromJsonAsync<OpenMatchRegistrationDto>();
        }

        public async Task DeleteRegistration(string id)
        {
            await Http.DeleteAsync($"{CONTROLLER}/MatchRegistration/{id}");
        }

        public async Task<List<OpenMatchRegistrationDto>> ReadRegistrations(Season season)
        {
            var response = await Http.GetAsync($"{CONTROLLER}/Registrations/{(int)season}");
            return await response.Content.ReadFromJsonAsync<List<OpenMatchRegistrationDto>>() ?? new List<OpenMatchRegistrationDto>();
        }
    }
}
