using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Models;
using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultService
    {
        List<MatchResult> GetResults(string matchId, ClubEvent match);
        List<MatchResult> GetMemberResults(List<string> matchIds, int membershipNumber);
        List<LeaguePosition> GetLeagueStandings(AggregateType aggType, Season season);
        List<AggregateWeight> GetAggregateWeights(AggregateType aggType, Season season);

        List<TrophyWinner> GetTrophyWinners(TrophyType trophyType, Season season);
    }
}