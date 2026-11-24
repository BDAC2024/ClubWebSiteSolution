using AnglingClubShared.Enums;
using AnglingClubShared.Extensions;
using AnglingClubShared.DTOs;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubWebServices.Controllers
{
    [Route("api/[controller]")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MembershipTrendsController : AnglingClubControllerBase
    {
        private static readonly TimeZoneInfo ClubTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        private readonly IMemberRepository _memberRepository;

        public MembershipTrendsController(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!CurrentUser.Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var asOfDate = GetClubDate();
            var members = await _memberRepository.GetMembers();
            var comparisons = MembershipTrendsReport.GetSeasonPairs(asOfDate)
                .Select(pair => MembershipTrendsReport.Compare(members, pair.Previous, pair.Current, asOfDate).Summary)
                .ToArray();

            return Ok(new MembershipTrendsDto(asOfDate, comparisons));
        }

        [HttpGet("{season:int}/non-renewals")]
        public async Task<IActionResult> ExportNonRenewals(Season season)
        {
            if (!CurrentUser.Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var asOfDate = GetClubDate();
            var pair = MembershipTrendsReport.GetSeasonPairs(asOfDate).SingleOrDefault(pair => pair.Current == season);
            if (pair.Current == Season.Unknown)
            {
                return NotFound();
            }

            var members = await _memberRepository.GetMembers();
            var (_, nonRenewals) = MembershipTrendsReport.Compare(members, pair.Previous, pair.Current, asOfDate);
            // Include a UTF-8 BOM so spreadsheet apps recognise accented names.
            var csv = new StringBuilder("\uFEFFEmail,Name,Membership number,Comments\r\n");

            var nameComparer = StringComparer.Create(CultureInfo.GetCultureInfo("en-GB"), ignoreCase: true);
            var rows = nonRenewals
                .OrderBy(member => EstimateSurname(member.Name), nameComparer)
                .ThenBy(member => member.Name, nameComparer)
                .ThenBy(member => member.MembershipNumber);

            foreach (var contact in rows)
            {
                var email = contact.Email.Trim();
                AppendCsvText(csv, email);
                csv.Append(',');
                AppendCsvText(csv, contact.Name);
                csv.Append(',').Append(contact.MembershipNumber.ToString(CultureInfo.InvariantCulture)).Append(',');
                AppendCsvText(csv, GetExportComment(email, contact.SeasonsActive.Any(activeSeason => activeSeason > season)));
                csv.Append("\r\n");
            }

            var fileName = $"membership-all-non-renewals-{pair.Previous.SeasonName().Replace('/', '-')}-to-{season.SeasonName().Replace('/', '-')}.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", fileName);
        }

        private static string GetExportComment(string email, bool rejoinedLater)
        {
            if (rejoinedLater)
            {
                return "Re-joined in a later season";
            }

            if (email.Length == 0)
            {
                return "Email missing";
            }

            return !email.Any(char.IsControl) && MailAddress.TryCreate(email, out var address) && address.Address == email
                ? "Email OK"
                : "Email invalid";
        }

        private static string EstimateSurname(string name)
        {
            name = name.Trim();
            var comma = name.IndexOf(',');
            if (comma > 0)
            {
                return name[..comma].TrimEnd();
            }

            var dot = name.LastIndexOf('.');
            if (dot >= 0 && dot + 1 < name.Length && !char.IsWhiteSpace(name[dot + 1]))
            {
                return name[(dot + 1)..];
            }

            return name.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        }

        private static void AppendCsvText(StringBuilder csv, string value)
        {
            csv.Append('"');
            // Keep spreadsheet formula prefixes as text, including after leading whitespace.
            var trimmed = value.TrimStart();
            if (trimmed.Length > 0 && trimmed[0] is '=' or '+' or '-' or '@')
            {
                csv.Append('\'');
            }

            csv.Append(value.Replace("\"", "\"\"")).Append('"');
        }

        private static DateOnly GetClubDate()
        {
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ClubTimeZone).DateTime);
        }
    }
}
