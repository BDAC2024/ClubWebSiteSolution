using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultService
    {
        List<MatchResult> GetResults(string matchId, MatchType matchType);
        List<LeaguePosition> GetLeagueStandings(MatchType matchType, Season season);
        List<AggregateWeight> GetAggregateWeights(AggregateWeightType aggWeightType, Season season);
    }
}