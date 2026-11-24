using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Services
{
    internal static class MembershipTrendsReport
    {
        internal static (Season Previous, Season Current)[] GetSeasonPairs(DateOnly asOfDate)
        {
            var seasons = EnumUtils.GetValues<Season>()
                .Where(season => season != Season.Unknown && DateOnly.FromDateTime(season.SeasonStarts()) <= asOfDate)
                .OrderBy(season => season.SeasonStarts())
                .TakeLast(6)
                .ToArray();

            var includesCurrentSeason = seasons.Length > 0 && asOfDate <= DateOnly.FromDateTime(seasons[^1].SeasonEnds());
            return seasons.Zip(seasons.Skip(1)).TakeLast(includesCurrentSeason ? 5 : 4).ToArray();
        }

        internal static (MembershipSeasonComparisonDto Summary, IReadOnlyList<Member> NonRenewals) Compare(
            IReadOnlyList<Member> members, Season previous, Season current, DateOnly asOfDate)
        {
            var individualMembers = members.Where(member => member.MembershipNumber >= 0);
            var previousMembers = individualMembers.Where(member => member.SeasonsActive.Contains(previous)).ToArray();
            var currentMembers = individualMembers.Where(member => member.SeasonsActive.Contains(current)).ToArray();

            var previousKeys = previousMembers.Select(member => member.DbKey).ToHashSet(StringComparer.Ordinal);
            var currentKeys = currentMembers.Select(member => member.DbKey).ToHashSet(StringComparer.Ordinal);
            var joined = currentMembers.Where(member => !previousKeys.Contains(member.DbKey)).ToArray();
            var lapsed = previousMembers.Where(member => !currentKeys.Contains(member.DbKey)).ToArray();

            var returned = joined.Count(member => member.SeasonsActive.Any(season => season != Season.Unknown && season < previous));

            var summary = new MembershipSeasonComparisonDto(
                previous, current, asOfDate <= DateOnly.FromDateTime(current.SeasonEnds()),
                previousMembers.Length, currentMembers.Length, previousMembers.Length - lapsed.Length, lapsed.Length,
                joined.Length - returned, returned);

            return (summary, lapsed);
        }
    }
}
