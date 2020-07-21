using Discord;
using Discord.Commands;
using Silicon.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silicon
{
    static class Utils
    {
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable
        {
            if (value.CompareTo(min) == -1) return min;
            if (value.CompareTo(max) == 1) return max;
            return value;
        }

        public static List<List<T>> DivideList<T>(this IEnumerable<T> collection, int chunkSize)
        {
            return collection.Select((v, i) => new { Index = i, Value = v })
                .GroupBy(x => x.Index / chunkSize)
                .Select(g => g.Select(x => x.Value).ToList()).ToList();
        }

        public static string Normalize(this TimeSpan span)
        {
            var strings = new List<string>();
            var numYears = 0;
            if (span.Days > 365)
            {
                numYears = span.Days / 365;
                strings.Add($"{numYears:D2} years");
            }

            if (span.Days > 0)
                strings.Add($"{(numYears > 0 ? span.Days - 365 * numYears : span.Days):D2} days");

            if (span.Hours > 0)
                strings.Add($"{span.Hours:D2} hours");

            if (span.Minutes > 0)
                strings.Add($"{span.Minutes:D2} minutes");

            if (span.Seconds > 0)
                strings.Add($"{span.Seconds:D2}{(span.Milliseconds > 0 ? $".{span.Milliseconds:D2}" : "")} seconds");

            return string.Join(", ", strings);
        }

        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);

        public static bool EqualsIgnoreCase(this string first, string sec) => first.Equals(sec, StringComparison.OrdinalIgnoreCase);

        public static bool ContainsIgnoreCase(this string first, string sec) => first.Contains(sec, StringComparison.OrdinalIgnoreCase);

        public static LogSource GetLogSrc(this LogMessage msg)
        {
            return msg.Source switch
            {
                "Rest" => LogSource.Rest,
                "Discord" => LogSource.Discord,
                "Gateway" => LogSource.Gateway,
                _ => LogSource.Unknown,
            };
        }

        public static string Highlight(this string str) => $"`{str}`";

        public static string Bold(this string str) => $"**{str}**";

        public static string UniqueName(this ModuleInfo module)
        {
            return module.Remarks ?? module.Name;
        }
    }
}
