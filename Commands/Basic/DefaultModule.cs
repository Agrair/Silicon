using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Silicon.Commands.Basic
{
    [Name("default")]
    public class DefaultModule : PandoraModule
    {
        [Command("ping")]
        [Summary("Gets response time for the bot")]
        public async Task Ping()
        {
            var sw = Stopwatch.StartNew();

            var msg = await ReplyAsync("Pong");

            sw.Stop();
            await msg.ModifyAsync(x => x.Content = $"Pong\nDiscord response time: {sw.ElapsedMilliseconds}ms");
        }

        [Command("source")]
        [Summary("Gets this bot's source code")]
        public Task Source() => ReplyAsync("https://github.com/Agrair/Silicon");

        [Command("csharp")]
        [Summary("The official C# discord")]
        public Task CsharpGuild() => ReplyAsync("https://discord.gg/ccyrDKv");

        [Command("botguild")]
        [Summary("The go-to support server for making bots")]
        public Task BotGuild() => ReplyAsync("https://discord.gg/discord-api");

        [Command("quickpy")]
        [Summary("Gets a link to a neato Python tutorial")]
        public Task QuickPython() => ReplyAsync("https://colab.research.google.com/github/GokuMohandas/practicalAI/blob/master/notebooks/01_Python.ipynb#scrollTo=sJ7NPGEKV6Ik");

        [Command("version")]
        public Task Version() =>
            ReplyAsync($"{(Core.BotVersion.Release ? "Release" : "Debug")} v{Core.BotVersion.Version}");
    }
}
