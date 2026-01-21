using System.ComponentModel;

namespace AnglingClubShared.Enums
{
    public enum DownloadType
    {
        inline = 0,
        attachment
    }

    public enum EventType
    {
        All = 0,
        Match,
        Work,
        Meeting,
    }

    public enum CalendarExportType
    {

        [Description("Matches, Meetings and Work Parties")]
        All = 0,

        [Description("All Matches and Work Parties")]
        AllMatches,

        [Description("Meetings and Work Parties")]
        Meetings,

        [Description("Spring League Matches, Junior Matches and Work Parties")]
        PondMatches,

        [Description("Club/River Matches and Work Parties")]
        RiverMatches,
    }

    public enum MatchType
    {
        [Description("Spring League")]
        Spring = 0,

        [Description("Club League")]
        Club,

        [Description("Junior League")]
        Junior,

        [Description("Ouse, Swale, Ure Team League")]
        OSU,

        Specials,

        [Description("Pairs")]
        Pairs,

        [Description("Evening League")]
        Evening,

        [Description("Visitors")]
        Visitors,

        [Description("Qualifier")]
        Qualifier
    }

    public enum OpenMatchType
    {
        [Description("Junior Open")]
        Junior = 0
    }

    public enum JuniorAgeGroup
    {
        [Description("Up to 12")]
        UpTo12 = 0,

        [Description("13 to 18")]
        ThirteenTo18
    }

    public enum AggregateType
    {
        [Description("Spring League")]
        Spring = 0,

        [Description("Club League - Rivers")]
        ClubRiver,

        [Description("Club League - Pond")]
        ClubPond,

        None,

        [Description("Pairs")]
        Pairs,

        [Description("Evening League")]
        Evening,

        [Description("Junior League")]
        Junior,

        [Description("Ouse, Swale, Ure Team League")]
        OSU,

        [Description("Pairs Points Asc")]
        PairsPointsAsc,

    }

    public enum WaterType
    {
        Stillwater = 0,
        River,
        Canal
    }

    public enum WaterAccessType
    {
        [Description("Members Only")]
        MembersOnly = 0,

        [Description("Day Tickets Available")]
        DayTicketsAvailable,

        [Description("Members and Guest Tickets")]
        MembersAndGuestTickets
    }

    public enum Season
    {
        [Description("Unknown,2019-04-01,2020-03-31")]
        Unknown = 0,

        [Description("2020/21,2020-04-01,2021-03-31")]
        S20To21 = 20,

        [Description("2021/22,2021-04-01,2022-03-31")]
        S21To22,

        [Description("2022/23,2022-04-01,2023-03-31")]
        S22To23,

        [Description("2023/24,2023-04-01,2024-03-31")]
        S23To24,

        [Description("2024/25,2024-04-01,2025-03-31")]
        S24To25,

        [Description("2025/26,2025-04-01,2026-03-31")]
        S25To26,

        [Description("2026/27,2026-04-01,2027-03-31")]
        S26To27,

        [Description("2027/28,2027-04-01,2028-03-31")]
        S27To28,

        [Description("2028/29,2028-04-01,2029-03-31")]
        S28To29,

        [Description("2029/30,2029-04-01,2030-03-31")]
        S29To30,

        [Description("2030/31,2030-04-01,2031-03-31")]
        S30To31,

        [Description("2031/32,2031-04-01,2032-03-31")]
        S31To32,

        [Description("2032/33,2032-04-01,2033-03-31")]
        S32To33,

        [Description("2033/34,2033-04-01,2034-03-31")]
        S33To34,

        [Description("2034/35,2034-04-01,2035-03-31")]
        S34To35,

        [Description("2035/36,2035-04-01,2036-03-31")]
        S35To36,

        [Description("2036/37,2036-04-01,2037-03-31")]
        S36To37,

        [Description("2037/38,2037-04-01,2038-03-31")]
        S37To38,

        [Description("2038/39,2038-04-01,2039-03-31")]
        S38To39,

        [Description("2039/40,2039-04-01,2040-03-31")]
        S39To40,

        [Description("2040/41,2040-04-01,2041-03-31")]
        S40To41,

        [Description("2041/42,2041-04-01,2042-03-31")]
        S41To42,

        [Description("2042/43,2042-04-01,2043-03-31")]
        S42To43,

        [Description("2043/44,2043-04-01,2044-03-31")]
        S43To44,

        [Description("2044/45,2044-04-01,2045-03-31")]
        S44To45,

        [Description("2045/46,2045-04-01,2046-03-31")]
        S45To46,

        [Description("2046/47,2046-04-01,2047-03-31")]
        S46To47,

        [Description("2047/48,2047-04-01,2048-03-31")]
        S47To48,

        [Description("2048/49,2048-04-01,2049-03-31")]
        S48To49,

        [Description("2049/50,2049-04-01,2050-03-31")]
        S49To50,

    }

    public enum RuleType
    {
        General = 0,

        Match,

        [Description("Junior / Intermediate Member - General")]
        JuniorGeneral,

        [Description("Junior / Intermediate Member - Match")]
        JuniorMatch,
    }

    public enum PaymentType
    {
        Membership = 0,
        GuestTicket,
        DayTicket,
        PondGateKey
    }

    public enum CheckoutType
    {
        [Description("payment")]
        Payment,

        [Description("setup")]
        Setup,

        [Description("subscription")]
        Subscription
    }

    public enum MembershipType
    {
        Adult12Months,
        Adult6Months,
        Junior12Months,
        Junior6Months,
        Intermediate12Months,
        SeniorCitizen12Months,
        Disabled12Months,
        Disabled6Months
    }

    public enum TrophyType
    {
        Senior,
        Junior
    }

    public enum DeviceSize
    {
        Unknown,
        Small,
        Medium,
        Large
    }


    public enum DocumentType
    {
        MeetingMinutes
    }

    public enum TmpFileType
    {
        BookPrinting,
        DisabledCertificate
    }
}
