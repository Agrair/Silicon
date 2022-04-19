using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class PollService
    {
        private readonly ConcurrentDictionary<ulong, PollData> _polls;

        public PollService()
        {
            _polls = new ConcurrentDictionary<ulong, PollData>();
        }


        public Task SimplePoll(SocketTextChannel channel, string name, string[] options)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            for (int i = 1; i <= options.Length; i++)
            {
                embed.AddField($"Option {i}", options[i]);
            }
            var id = SnowflakeUtils.ToSnowflake(System.DateTimeOffset.UtcNow);
            embed.WithFooter("Poll ID: " + id);

            _polls.TryAdd(id, new PollData(name, options));

            return channel.SendMessageAsync(embed: embed.Build());
        }

        public Task Vote(SocketCommandContext ctx, ulong id, int option)
        {
            if (!_polls.TryGetValue(id, out var poll))
            {
                return ctx.Channel.SendMessageAsync("No poll with that ID is active.");
            }

            if (option < 0 || option > poll.Options.Length)
            {
                return ctx.Channel.SendMessageAsync("Enter a valid option");
            }

            poll.Votes[option - 1].Add(id);

            return Task.CompletedTask;
        }

        public Task ClosePoll(SocketCommandContext ctx, SocketTextChannel channel, ulong id) {
            if (!_polls.TryRemove(id, out var poll)) {
                return ctx.Channel.SendMessageAsync("No poll with that ID is active.");
            }

            var embed = new EmbedBuilder();
            embed.WithTitle(poll.Name);

            var tuples = poll.Votes
                .Select((set, i) => (set.Count, i))
                .OrderByDescending(tuple => tuple.Count);

            foreach (var (Count, i) in tuples) {
                embed.AddField(poll.Options[i], Count);
            }

            embed.WithFooter("Poll ID: " + id);

            return channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
