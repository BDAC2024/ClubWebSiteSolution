using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMemberRepository
    {
        string SiteUrl { get; }
        Task AddOrUpdateMember(Member member);
        Task<List<Member>> GetMembers(Season? activeSeason = null, bool forMatches = false);
    }
}