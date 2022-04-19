using Discord;
using Discord.Commands;
using Silicon.Commands.Commons;
using Silicon.Helpers;
using System.Net.Http;
using System.Threading.Tasks;

namespace Silicon.Commands.Imaging
{
    [Name("modify")]
    [Ratelimit(2, 5)]
    public class ModifyModule : SiliconModule
    {
        public HttpClient Client { get; set; }

        [Command("invert", RunMode = RunMode.Async)]
        [Summary("Inverts someone's avatar.")]
        [Priority(1)]
        public Task Invert(IUser user = null)
        {
            var person = user ?? Context.User;
            return Invert(person.GetAvatarUrl() ?? person.GetDefaultAvatarUrl());
        }

        [Command("invert", RunMode = RunMode.Async)]
        [Summary("Inverts the image associated with the specified url.")]
        public async Task Invert(string url)
        {
            var img = Client.GetStreamAsync(url);
            var msg = await Context.Channel.SendMessageAsync("Generating inversion...");
            await Context.Channel.SendFileAsync(ImageHelper.Invert(await img), "inverted.png");
            await msg.DeleteAsync();
        }
    }
}
