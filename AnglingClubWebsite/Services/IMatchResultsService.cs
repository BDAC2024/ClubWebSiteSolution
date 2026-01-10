using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;

namespace AnglingClubWebsite.Services
{
    public interface IMatchResultsService
    {
        Task<List<MatchResultOutputDto>?> GetResultsForMatch(string matchId);
    }
}