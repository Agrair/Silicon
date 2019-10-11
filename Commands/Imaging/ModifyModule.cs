using Discord.Commands;
using Discord.WebSocket;
using Silicon.Commands.Commons;
using Silicon.Helpers;
using System.Threading.Tasks;

namespace Silicon.Commands.Imaging
{
    [Name("modify")]
    [Ratelimit(5, 10)]
    public class ModifyModule : PandoraModule
    {
        [Command("invert", RunMode = RunMode.Async)]
        [Summary("Inverts someone's avatar.")]
        [Priority(1)]
        public Task Invert(SocketGuildUser user = null) =>
            Invert((user ?? (Context.User as SocketGuildUser)).GetAvatarUrl());

        [Command("invert", RunMode = RunMode.Async)]
        [Summary("Inverts the image associated with the specified url.")]
        public async Task Invert(string url)
        {
            await Context.Channel.SendFileAsync(ImageHelper.Invert(await NetHelper.GetHttpStream(url), url), "inverted.png");
        }
    }
}
