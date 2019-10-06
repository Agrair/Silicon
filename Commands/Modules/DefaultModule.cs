using Discord.Commands;
using Silicon.Modules.Commons;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Silicon.Commands.Modules
{
    [Name("default")]
    [BotChannelsOnly]
    public class DefaultModule : PandoraModule
    {
        [Command("ping")]
        [Summary("Gets response time for the bot.")]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();

            var msg = await ReplyAsync("Pong");

            sw.Stop();
            await msg.ModifyAsync(x => x.Content = $"Pong\nDiscord response time: {sw.ElapsedMilliseconds}ms");
        }

        [Command("csharp")]
        [Summary("The official C# discord.")]
        public async Task CsharpGuild() => await ReplyAsync("https://discord.gg/ccyrDKv");

        [Command("botguild")]
        [Summary("The go-to support server for making bots.")]
        public async Task BotGuild() => await ReplyAsync("https://discord.gg/discord-api");

        [Command("quickpy")]
        [Summary("Gets a link to a neato Python tutorial.")]
        public async Task QuickPython() => await ReplyAsync("https://colab.research.google.com/github/GokuMohandas/practicalAI/blob/master/notebooks/01_Python.ipynb#scrollTo=sJ7NPGEKV6Ik");

        [Command("version")]
        public async Task Version() =>
            await ReplyAsync($"{(Core.BotVersion.Release ? "Release" : "Debug")} v{Core.BotVersion.Version}");
    }
}
