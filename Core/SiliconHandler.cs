using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Commands.Commons;
using Silicon.Helpers;
using Silicon.Models.Enums;
using Silicon.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Silicon.Core
{
    public class SiliconHandler
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private readonly UserService _user;
        private readonly TextCrunchService _text;
        private readonly TriviaService _trivia;

        public static bool Ready { get; set; }

        public SiliconHandler(IServiceProvider services)
        {
            this.services = services;

            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            _user = services.GetRequiredService<UserService>();
            _text = services.GetRequiredService<TextCrunchService>();
            _trivia = services.GetRequiredService<TriviaService>();
        }

        public async Task StartAsync()
        {
            var sw = Stopwatch.StartNew();

            _commandService.AddTypeReader<Color>(new ColorReader());
            var modules = await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            sw.Stop();

            await LoggingHelper.Log(LogSeverity.Info, LogSource.Silicon,
                $"Loaded {modules.Count()} modules and {modules.Sum(m => m.Commands.Count)}" +
                $" commands loaded in {sw.ElapsedMilliseconds}ms.");

            _client.Log += async m => await LoggingHelper.Log(m);
            _client.Ready += ClientReady;
            _client.MessageReceived += ClientMessageReceieved;
            _client.UserLeft += ClientUserLeft;

            _commandService.Log += async m => await LoggingHelper.Log(m);
            _commandService.CommandExecuted += CommandExecuted;
        }

        private Task ClientUserLeft(SocketGuildUser user)
        {
            if (user.MutualGuilds.Count == 0) _user.RemoveUser(user.Id);
            return Task.CompletedTask;
        }

        private Task ClientReady()
        {
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

            int argPos = 0;
            if (msg.Content.Length > 2 
                && (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) 
                || msg.HasStringPrefix("s|", ref argPos)))
            {
                var context = new SocketCommandContext(_client, msg);

                await _commandService.ExecuteAsync(context, argPos, services);
            }
            else if (_trivia.Channel?.Id == s.Channel.Id)
            {
                if (await _trivia.CheckAnswer(msg))
                {
                    await msg.Channel.SendMessageAsync(embed: _trivia.GetEmbed(msg.Author).Build());
                }
            }
            else
                _ = _text.TryHaste(msg);
        }

        private async Task CommandExecuted(Optional<CommandInfo> cmd, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && !result.ErrorReason.IsNullOrWhitespace() && result.ErrorReason != "Unknown command.")
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                await LoggingHelper.Log(LogSeverity.Warning, LogSource.Module, result.ErrorReason,
                    result is ExecuteResult execResult ? execResult.Exception : null);
            }
        }
    }
}