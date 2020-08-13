using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Silicon.Commands
{
    public abstract class SiliconModule : ModuleBase<SocketCommandContext>
    {
        public Task ReplyAsync(Embed embed) => ReplyAsync(null, false, embed);
    }
}
