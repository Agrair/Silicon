using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Silicon.Core
{
    public class SiliconBot
    {
        private static readonly ServiceProvider _services = BuildServiceProvider();
        private readonly DiscordSocketClient _client = _services.GetRequiredService<DiscordSocketClient>();
        private readonly SiliconHandler _handler = _services.GetRequiredService<SiliconHandler>();

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

                .AddSingleton(new LiteDB.LiteDatabase("data.db"))
                .AddSingleton<TagService>()
                .AddSingleton<UserService>()
                .AddSingleton<WebService>()

                .AddSingleton<CommandHandlerService>()

                //module services
                .AddSingleton<ModeratorService>()
                .AddSingleton<TextCrunchService>()
                .AddSingleton<InteractiveService>()

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
            await _client.SetGameAsync("Opening the box");
            await _handler.StartAsync();

            string cmd = Console.ReadLine();
            while (cmd != "stop")
            {
                cmd = Console.ReadLine();
            }
            await ShutdownAsync();
        }

        private async Task ShutdownAsync()
        {
            await _client.SetStatusAsync(UserStatus.Invisible);
            await _client.LogoutAsync();
            await _client.StopAsync();
            Dispose();
            Environment.Exit(0);

            void Dispose()
            {
                foreach (var service in _services.GetServices<IDisposable>()) service.Dispose();
                _services.Dispose();
                _client.Dispose();
            }
        }
    }
}
