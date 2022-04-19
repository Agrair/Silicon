using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Commands.Basic
{
    [Group("poll")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public class PollModule : SiliconModule
    {
        private static readonly Emoji Down = new Emoji("👎");
        private static readonly Emoji Up = new Emoji("👍");


        public PollService Polling { get; set; }


        [Command("bool")]
        [Summary("A simple yes or no poll")]
        public async Task BooleanPoll(SocketTextChannel channel, string name, [Remainder] string text)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(name);
            embed.WithDescription(text);
            var msg = await channel.SendMessageAsync(embed: embed.Build());
            await msg.AddReactionAsync(Down);
            await msg.AddReactionAsync(Up);
        }

        [Command("simple")]
        [Summary("A poll that will keep going until it's ended")]
        public Task SimplePoll(SocketTextChannel channel, string name, params string[] options)
        {
            return Polling.SimplePoll(channel, name, options);
        }

        [Command("vote")]
        [Summary("Vote for one of the options in an on-going poll")]
        public Task Vote(ulong poll, int option)
        {
            return Polling.Vote(Context, poll, option);
        }

        [Command("close")]
        [Summary("Close a poll")]
        public Task ClosePoll(SocketTextChannel channel, ulong id) {
            return Polling.ClosePoll(Context, channel, id);
        }
    }
}
