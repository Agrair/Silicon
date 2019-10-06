using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Commands.Modules
{
    [Name("admin")]
    public class AdminModule : PandoraModule
    {
        [Group("channel")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public class ChannelModule : PandoraModule
        {
            public ModeratorService Moderator { get; set; }

            [Command("addbot")]
            public async Task AddBotChannel(SocketGuildChannel channel)
            {
                Moderator.AddBotChannel(channel);
                await ReplyAsync($"Added bot channel #{channel.Name}.");
            }

            [Command("removebot")]
            public async Task RemoveBotChannel(SocketGuildChannel channel)
            {
                Moderator.RemoveBotChannel(channel);
                await ReplyAsync($"Removed bot channel #{channel.Name}.");
            }
        }
    }
}