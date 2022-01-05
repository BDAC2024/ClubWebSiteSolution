using System.ComponentModel;

namespace AnglingClubWebServices.Interfaces
{

    public enum EventType
    {
        All = 0,
        Match,
        Work,
        Meeting,
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

        Specials
    }

    public enum AggregateWeightType
    {
        [Description("Spring League")]
        Spring = 0,

        [Description("Club League - Rivers")]
        ClubRiver,

        [Description("Club League - Pond")]
        ClubPond,

        None,
    }

    public enum WaterType
    {
        Stillwater = 0,
        River,
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
        [Description("2020/21,2020-03-15,2021-03-14")]
        S20To21 = 20,

        [Description("2021/22,2021-03-15,2022-03-14")]
        S21To22,

        [Description("2022/23,2022-03-15,2023-03-14")]
        S22To23,

        [Description("2023/24,2023-03-15,2024-03-14")]
        S23To24,

        [Description("2024/25,2024-03-15,2025-03-14")]
        S24To25,

        [Description("2025/26,2025-03-15,2026-03-14")]
        S25To26,

        [Description("2026/27,2026-03-15,2027-03-14")]
        S26To27,

        [Description("2027/28,2027-03-15,2028-03-14")]
        S27To28,

        [Description("2028/29,2028-03-15,2029-03-14")]
        S28To29,

        [Description("2029/30,2029-03-15,2030-03-14")]
        S29To30,

        [Description("2030/31,2030-03-15,2031-03-14")]
        S30To31,

        [Description("2031/32,2031-03-15,2032-03-14")]
        S31To32,

        [Description("2032/33,2032-03-15,2033-03-14")]
        S32To33,

        [Description("2033/34,2033-03-15,2034-03-14")]
        S33To34,

        [Description("2034/35,2034-03-15,2035-03-14")]
        S34To35,

        [Description("2035/36,2035-03-15,2036-03-14")]
        S35To36,

        [Description("2036/37,2036-03-15,2037-03-14")]
        S36To37,

        [Description("2037/38,2037-03-15,2038-03-14")]
        S37To38,

        [Description("2038/39,2038-03-15,2039-03-14")]
        S38To39,

        [Description("2039/40,2039-03-15,2040-03-14")]
        S39To40,

        [Description("2040/41,2040-03-15,2041-03-14")]
        S40To41,

        [Description("2041/42,2041-03-15,2042-03-14")]
        S41To42,

        [Description("2042/43,2042-03-15,2043-03-14")]
        S42To43,

        [Description("2043/44,2043-03-15,2044-03-14")]
        S43To44,

        [Description("2044/45,2044-03-15,2045-03-14")]
        S44To45,

        [Description("2045/46,2045-03-15,2046-03-14")]
        S45To46,

        [Description("2046/47,2046-03-15,2047-03-14")]
        S46To47,

        [Description("2047/48,2047-03-15,2048-03-14")]
        S47To48,

        [Description("2048/49,2048-03-15,2049-03-14")]
        S48To49,

        [Description("2049/50,2049-03-15,2050-03-14")]
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

}
