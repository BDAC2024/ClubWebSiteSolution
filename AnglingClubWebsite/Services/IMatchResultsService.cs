using AnglingClubShared.DTOs;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;

namespace AnglingClubWebsite.Services
{
    public interface IMatchResultsService
    {
        Task<List<MatchResultOutputDto>?> GetResultsForMatch(string matchId);
        Task<List<LeaguePosition>?> GetLeaguePositions(AggregateType aggType, Season season);
        Task<List<AggregateWeight>?> GetAggreateWeights(AggregateType aggType, Season season);
        Task<List<TrophyWinner>?> GetTrophyWinners(TrophyType trophyType, Season season);
    }
}