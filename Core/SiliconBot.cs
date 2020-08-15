using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Silicon.Core
{
    public class SiliconBot
    {
        private static readonly ServiceProvider _services = BuildServiceProvider();
        private readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly SiliconHandler _handler = _services.GetRequiredService<SiliconHandler>();
        private readonly TriviaService _trivia = _services.GetRequiredService<TriviaService>();

        private static ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = BotVersion.Release
                        ? LogSeverity.Info
                        : LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    ConnectionTimeout = 10000,
                    MessageCacheSize = 50
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    IgnoreExtraArgs = true,
                    DefaultRunMode = RunMode.Sync
                }))

                //data services
                .AddSingleton(new LiteDB.LiteDatabase("data.db"))
                .AddSingleton<TagService>()
                .AddSingleton<UserService>()

                //module services
                .AddSingleton<TextCrunchService>()
                .AddSingleton<InteractiveService>()

                //other services
                .AddSingleton<HttpClient>()
                .AddSingleton<TriviaService>()

                .AddSingleton<SiliconHandler>()
                .BuildServiceProvider();
        }

        public static Task StartAsync() => new SiliconBot().LoginAsync();

        private async Task LoginAsync()
        {
            Console.Title = "Silicon";

            await _client.LoginAsync(TokenType.Bot, File.ReadAllText("D:/repos/token.txt"));
            await _client.StartAsync();

            await _client.SetStatusAsync(UserStatus.Online);
            await _handler.StartAsync();

            string cmd = Console.ReadLine();
            while (cmd != "stop")
            {
                var args = cmd.Split(' ');
                int index = 0;
                try
                {
                    do
                    {
                        switch (args[index++])
                        {
                            case "-setready":
                                SiliconHandler.Ready = bool.Parse(args[index++]);
                                break;
                            case "--checkready":
                                Console.WriteLine(SiliconHandler.Ready);
                                break;
                            case "-triviachannel":
                                _trivia.SetChannel(_client.GetChannel(ulong.Parse(args[index++])) as SocketGuildChannel);
                                break;
                            case "--stoptrivia":
                                _trivia.StopTrivia();
                                break;
                            default:
                                Console.WriteLine($"Unknown command `{cmd}`");
                                break;
                        }
                    }
                    while (index < args.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                cmd = Console.ReadLine();
            }
            await ShutdownAsync();
        }

        private async Task ShutdownAsync()
        {
            await _client.SetStatusAsync(UserStatus.Invisible);
            await _client.LogoutAsync();
            await _client.StopAsync();
            _services.GetRequiredService<HttpClient>().Dispose();
            _services.GetRequiredService<TextCrunchService>().Dispose();
            Environment.Exit(0);
        }
    }
}
