using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Commands.Commons;
using Silicon.Helpers;
using Silicon.Models;
using Silicon.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Silicon.Core
{
    public class SiliconHandler
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private readonly UserService _user;

        private readonly CommandHandlerService _commandHandler;

        public static bool Ready { get; private set; }

        public SiliconHandler(IServiceProvider services)
        {
            this.services = services;

            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            _user = services.GetRequiredService<UserService>();

            _commandHandler = services.GetRequiredService<CommandHandlerService>();
        }

        public async Task StartAsync()
        {
            var sw = Stopwatch.StartNew();

            _commandService.AddTypeReader<Color>(new ColorReader());
            var m = await _commandService.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), services);

            sw.Stop();

            await LoggingHelper.Log(LogSeverity.Info, LogSource.Silicon,
                $"Loaded {m.Count()} modules and {m.Sum(m => m.Commands.Count)}" +
                $" commands loaded in {sw.ElapsedMilliseconds}ms.");

            _client.Log += async m => await LoggingHelper.Log(m);
            _client.Ready += ClientReady;
            _client.MessageReceived += ClientMessageReceieved;
            _client.UserLeft += ClientUserLeft;

            _commandService.Log += async m => await LoggingHelper.Log(m);
            _commandService.CommandExecuted += async (command, context, r) => await CommandHandlerService.CommandExecuted(context, r);
        }

        private Task ClientUserLeft(SocketGuildUser user) => _user.RemoveUser(user.Id);

        private Task ClientReady()
        {
            //may be called more than once
            Ready = true;
            //ready.txt is moved to output bin
            Console.Out.WriteLine(Program.ready);
            return Task.CompletedTask;
        }

        private async Task ClientMessageReceieved(SocketMessage s)
        {
            if (!Ready) return;
            if (!(s is SocketUserMessage msg)) return;
            if (msg.Author.IsBot || msg.Author.IsWebhook) return;
            if (msg.Content.Length <= 1 || !char.IsLetter(msg.Content[1])) return;

            int argPos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasCharPrefix('|', ref argPos))
            {
                await _commandHandler.HandleCmdAsync(msg, argPos);
            }
        }
    }
}