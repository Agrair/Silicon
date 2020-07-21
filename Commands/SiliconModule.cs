using Discord;
using Discord.Commands;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    public abstract class SiliconModule : ModuleBase<SocketCommandContext>
    {
        public InteractiveService Interactive { get; set; }

        public Task ReplyAsync(Embed embed) => ReplyAsync(null, false, embed);
    }
}
