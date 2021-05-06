using AnglingClubWebServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMemberRepository
    {
        Task AddOrUpdateMember(Member member);
        Task<List<Member>> GetMembers();
    }
}