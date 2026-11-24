using AnglingClubShared.DTOs;
using AnglingClubShared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnglingClubWebServices.Helpers
{
    internal static class MemberMergeCandidates
    {
        internal static MemberMergeCandidate[] Find(IEnumerable<Member> members)
        {
            var candidates = new List<MemberMergeCandidate>();
            var repeatedNumbers = members.Where(member => member.MembershipNumber > 0)
                .GroupBy(member => member.MembershipNumber)
                .Where(group => group.Skip(1).Any());

            foreach (var group in repeatedNumbers)
            {
                var names = FindPairs(group.ToArray())
                    .SelectMany(pair => new[] { pair.First.Name, pair.Second.Name })
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(name => name, StringComparer.Ordinal)
                    .ToArray();
                if (names.Length > 0)
                {
                    candidates.Add(new MemberMergeCandidate(group.Key, names));
                }
            }

            return candidates.OrderBy(candidate => candidate.Names[0], StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.MembershipNumber)
                .ToArray();
        }

        internal static IEnumerable<(Member First, Member Second)> FindPairs(IReadOnlyList<Member> records)
        {
            var names = records.Select(member => NameParts(member.Name)).ToArray();
            for (var first = 0; first < records.Count - 1; first++)
            {
                for (var second = first + 1; second < records.Count; second++)
                {
                    if (NamesLookRelated(names[first], names[second]))
                    {
                        yield return (records[first], records[second]);
                    }
                }
            }
        }

        internal static bool NamesLookRelated(string first, string second)
        {
            return NamesLookRelated(NameParts(first), NameParts(second));
        }

        private static string[] NameParts(string name)
        {
            var parts = Regex.Split(name.ToUpperInvariant(), @"[^\p{L}\p{M}\p{N}]+")
                .Where(part => part.Length > 0).ToArray();
            return parts.Length > 1 && parts[0] is "MR" or "MRS" or "MS" or "MISS" or "MST" or "MASTER" or "DR"
                ? parts[1..]
                : parts;
        }

        private static bool NamesLookRelated(string[] first, string[] second)
        {
            if (first.Length == 0 || second.Length == 0)
            {
                return false;
            }

            if (string.Concat(first) == string.Concat(second))
            {
                return true;
            }

            if (first.Length < 2 || second.Length < 2)
            {
                return false;
            }

            var firstGivenName = first[0];
            var secondGivenName = second[0];
            var firstSurname = first[^1];
            var secondSurname = second[^1];
            var relatedGivenNames = firstGivenName == secondGivenName ||
                (firstGivenName[0] == secondGivenName[0] && (firstGivenName.Length == 1 || secondGivenName.Length == 1));

            if (firstSurname == secondSurname)
            {
                return relatedGivenNames || (firstGivenName.Length == 1 && secondGivenName.Length == 1);
            }

            return relatedGivenNames && firstSurname.Length >= 4 && secondSurname.Length >= 4 &&
                firstSurname.AsSpan(0, 2).SequenceEqual(secondSurname.AsSpan(0, 2)) &&
                EditDistance(firstSurname, secondSurname) <= 2;
        }

        private static int EditDistance(string first, string second)
        {
            var distances = Enumerable.Range(0, second.Length + 1).ToArray();
            for (var row = 1; row <= first.Length; row++)
            {
                var diagonal = distances[0];
                distances[0] = row;
                for (var column = 1; column <= second.Length; column++)
                {
                    var above = distances[column];
                    var substitution = diagonal + (first[row - 1] == second[column - 1] ? 0 : 1);
                    distances[column] = Math.Min(Math.Min(above, distances[column - 1]) + 1, substitution);
                    diagonal = above;
                }
            }

            return distances[^1];
        }
    }
}
