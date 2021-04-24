using AnglingClubWebServices.Models;
using System.Collections.Generic;

namespace AnglingClubWebServices.Interfaces
{
    public interface IMatchResultService
    {
        List<MatchResult> GetResults(string matchId);
    }
}