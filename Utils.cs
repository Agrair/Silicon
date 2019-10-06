using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static T NextIndex<T>(this IEnumerable<T> arr)
        {
            return arr.ElementAt(Program.rand.Next(arr.Count()));
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

        public static bool EqualsIgnoreCase(this string first, string sec) => first.ToLower() == sec.ToLower();

        public static bool ContainsIgnoreCase(this string first, string sec) => first.ToLower().Contains(sec.ToLower());

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

        public static SiliconChannel ToDatabaseValue(this SocketGuildChannel c) => new SiliconChannel
        {
            Name = c.Name,
            Snowflake = c.Id
        };

        public static Task<IUserMessage> SendToAsync(this Embed e, IMessageChannel c) =>
            c.SendMessageAsync(string.Empty, false, e);

        public static Task AddEveryonePermAsync(this IChannel channel, OverwritePermissions perms)
        {
            var guildChannel = channel as IGuildChannel;
            return guildChannel.AddPermissionOverwriteAsync(guildChannel.Guild.EveryoneRole, perms);
        }

        public static string ModuleName(this ModuleInfo module) => module.Name.Bold();

        public static string Highlight(this string str) => $"`{str}`";

        public static string Bold(this string str) => $"**{str}**";
    }
}
