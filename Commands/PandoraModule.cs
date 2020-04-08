using Discord;
using Discord.Commands;
using Silicon.Services;
using System;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    public abstract class PandoraModule : ModuleBase<SocketCommandContext>
    {
        public InteractiveService Interactive { get; set; }

        public async Task ReplyAndDeleteAsync(TimeSpan deleteTime, string message, Action final, Embed embed = null)
        {
            var m = await ReplyAsync(message, embed: embed);
            _ = Task.Delay(deleteTime).ContinueWith(_ => m.DeleteAsync()).ContinueWith(_ => final?.Invoke());
        }

        public Task ReplyAsync(Embed embed) => ReplyAsync(null, false, embed);
    }
}
