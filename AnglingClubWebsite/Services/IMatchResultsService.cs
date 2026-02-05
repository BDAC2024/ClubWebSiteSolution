using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;

namespace AnglingClubWebsite.Services
{
    public interface IMatchResultsService
    {
        Task<List<MatchResultOutputDto>?> GetResultsForMatch(string matchId);
        Task<List<LeaguePosition>?> GetLeaguePositions(AggregateType aggType, Season season);
    }
}