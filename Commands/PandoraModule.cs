using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    public abstract class PandoraModule : ModuleBase<PandoraContext>
    {
        public InteractiveService Interactive { get; }

        public async Task ReplyAndDeleteAsync(TimeSpan deleteTime, string message = null, Embed embed = null)
        {
            var m = await ReplyAsync(message: message, embed: embed);
            _ = Task.Delay(deleteTime).ContinueWith(_ => m.DeleteAsync());
        }

        public async Task<SocketMessage> NextMessageAsync(CancellationTokenSource source, TimeSpan timeout)
        {
            return await Interactive.GetResponseAsync(Context, timeout, m =>
            {
                if (m.Author.Id == Context.User.Id
                    || (m.Author is SocketGuildUser guildUser && guildUser.GuildPermissions.ManageMessages))
                {
                    if (m.Content.EqualsIgnoreCase("stop"))
                    {
                        source.Cancel();
                        return false;
                    }
                    return true;
                }
                return false;
            }, source.Token);
        }

        public async Task ReplyAsync(Embed embed) => await ReplyAsync(null, false, embed, null);
    }
}
