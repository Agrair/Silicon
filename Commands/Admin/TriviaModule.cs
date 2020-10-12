using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Commands.Admin
{
    [RequireUserPermission(Discord.GuildPermission.ManageChannels)]
    public class TriviaModule : SiliconModule
    {
        public TriviaService Trivia { get; set; }

        [Command("launchtrivia")]
        public Task LaunchTrivia(SocketGuildChannel channel)
        {
            Trivia.SetChannel(Context.Guild.Id, channel);
            return ReplyAsync($"Set the current server's trivia channel to <#{channel.Id}>");
        }

        [Command("stoptrivia")]
        public Task StopTrivia()
        {
            return ReplyAsync(Trivia.StopTrivia(Context.Guild.Id)
                ? "No trivia currently active"
                : "Stopped trivia for the current server");
        }
    }
}
