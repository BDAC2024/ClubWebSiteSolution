using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using System.Net.Http.Json;

namespace AnglingClubWebsite.Services
{
    public class MembershipTrendsService : DataServiceBase
    {
        public MembershipTrendsService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public Task<MembershipTrendsDto?> GetReport()
        {
            return Http.GetFromJsonAsync<MembershipTrendsDto>("MembershipTrends");
        }

        public Task<byte[]> GetNonRenewalsCsv(Season season)
        {
            return Http.GetByteArrayAsync($"MembershipTrends/{(int)season}/non-renewals");
        }
    }
}
