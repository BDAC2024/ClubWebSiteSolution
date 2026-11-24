using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.DTOs;
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
        Task<MemberMergePreview> PreviewMerge(MemberMergeRequest request);
        Task ApplyMerge(MemberMergeRequest request);
        Task<PendingMemberMerge[]> GetPendingMerges();
        Task FinalizeMerge(FinalizeMemberMergeRequest request);
    }
}
