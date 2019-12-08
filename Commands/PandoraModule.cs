using Discord;
using Discord.Commands;
using Silicon.Commands.Commons;
using Silicon.Services;
using System;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    [IsEnabled]
    public abstract class PandoraModule : ModuleBase<SocketCommandContext>
    {
        public InteractiveService Interactive { get; set; }

        public async Task ReplyAndDeleteAsync(TimeSpan deleteTime, string message = null, Embed embed = null, Action final = null)
        {
            var m = await ReplyAsync(message: message, embed: embed);
            _ = Task.Delay(deleteTime).ContinueWith(_ => m.DeleteAsync()).ContinueWith(_ => final?.Invoke());
        }

        public Task ReplyAsync(Embed embed) => ReplyAsync(null, false, embed);
    }
}
