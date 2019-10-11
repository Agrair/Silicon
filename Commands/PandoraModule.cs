using Discord;
using Discord.Commands;
using Silicon.Commands.Commons;
using Silicon.Services;
using System;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    [Ratelimit(5, 5)]
    public abstract class PandoraModule : ModuleBase<PandoraContext>
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
