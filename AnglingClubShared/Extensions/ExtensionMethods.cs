using AnglingClubShared.Enums;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;


namespace AnglingClubShared.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the "Description" attribute from an enum. Returns the name of the enum value if no description is available.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flagSeparator">Optional, separator for flag enums to override the default</param>
        /// <returns></returns>
        public static string EnumDescription(this Enum value, string flagSeparator = " - ")
        {
            //pull out each value in case of flag enumeration
            var values = value.ToString().Split(',').Select(s => s.Trim());
            var type = value.GetType();

            return string.Join(flagSeparator, values.Select(enumValue => type.GetMember(enumValue)
               .FirstOrDefault()
               ?.GetCustomAttribute<DescriptionAttribute>()
               ?.Description
               ?? enumValue.ToString()));
        }

        /// <summary>
        /// Returns the season name from [Description("2021/22,2021-04-01,2022-03-31")]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SeasonName(this Season value)
        {
            var name = seasonParts(value)[0];

            return name;
        }

        /// <summary>
        /// Returns the season start date from [Description("2021/22,2021-04-01,2022-03-31")]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime SeasonStarts(this Season value)
        {
            var starts = DateTime.Parse(seasonParts(value)[1]);

            return starts;
        }

        /// <summary>
        /// Returns the season end date from [Description("2021/22,2021-04-01,2022-03-31")]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime SeasonEnds(this Season value)
        {
            var ends = DateTime.Parse(seasonParts(value)[2]).AddHours(23).AddMinutes(59);

            return ends;
        }


        public static string Ordinal(this int num)
        {
            if (num <= 0)
            {
                return "";
            }

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        /// <summary>
        /// Returns a date of the form "Sat 15th Dec 2024"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string PrettyDate(this DateTime value)
        {
            return value.ToString("ddd ") + value.Day.Ordinal() + value.ToString(" MMM yyyy");
        }

        /// <summary>
        /// Returns a date of the form "18 Dec 25"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string BdacDate(this DateTime value)
        {
            return value.ToString("dd MMM yy");
        }

        /// <summary>
        /// Splits an enum desc of [Description("2021/22,2021-04-01,2022-03-31")] into
        /// comma separted parts
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        private static string[] seasonParts(Season season)
        {
            var desc = season.EnumDescription();
            var parts = desc.Split(",");

            return parts;
        }

        /// <summary>
        /// Adds IsNullOrEmpty to string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        /// <summary>
        /// Gets a boolean claim from a JWT token, returns false if not found or invalid
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetBoolClaim(this JwtSecurityToken principal, string type)
        {
            var requiredClaim = principal.Claims.FirstOrDefault(c => c.Type == type);

            if (requiredClaim != null &&
                bool.TryParse(requiredClaim.Value, out var requiredValue))
            {
                return requiredValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Takes a float version of a weight and returns in Lb & Oz
        /// </summary>
        /// <param name="weightDecimal"></param>
        /// <returns></returns>
        public static string WeightAsString(this float weightDecimal)
        {
            var wt = "DNW";

            if (weightDecimal > 0)
            {
                var wtLb = Math.Floor(weightDecimal);
                var wtOz = Math.Round((weightDecimal - wtLb) * 16);
                wt = $"{wtLb}lb {wtOz}oz";
            }

            return wt;

        }

    }
}
