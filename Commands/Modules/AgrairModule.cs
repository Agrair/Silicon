using Discord.Commands;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Commands.Modules
{
    [RequireOwner]
    [Group("debug")]
    public class AgrairModule : PandoraModule
    {
        public TextCrunchService Haste { get; set; }

        [Command("activatehaste", RunMode = RunMode.Async)]
        public async Task ActivateHastebin()
        {
            if (Haste.OfflineCheck(out string site)) await ReplyAsync("Now running on " + site);
            else await ReplyAsync("Nope still down");
        }
    }
}
