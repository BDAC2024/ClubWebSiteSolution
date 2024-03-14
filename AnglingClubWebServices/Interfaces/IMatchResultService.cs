using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultService
    {
        List<MatchResult> GetResults(string matchId, MatchType matchType);
        List<LeaguePosition> GetLeagueStandings(AggregateType aggType, Season season);
        List<AggregateWeight> GetAggregateWeights(AggregateType aggType, Season season);
    }
}