using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultService
    {
        List<MatchResult> GetResults(string matchId);
        List<LeaguePosition> GetLeagueStandings(MatchType matchType, Season season);
        List<AggregateWeight> GetAggregateWeights(MatchType matchType, Season season);
    }
}