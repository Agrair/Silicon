using Discord.Commands;
using Discord.WebSocket;

namespace Silicon.Commands
{
    public class PandoraContext : SocketCommandContext
    {
        public PandoraContext(DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
        }
    }
}
