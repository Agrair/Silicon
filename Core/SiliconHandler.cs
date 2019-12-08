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
using System.Text.RegularExpressions;
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

        public static bool Ready { get; private set; }

        public SiliconHandler(IServiceProvider services)
        {
            this.services = services;

            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            _user = services.GetRequiredService<UserService>();

            _text = services.GetRequiredService<TextCrunchService>();
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
            if (msg.Content.Length <= 2 || !char.IsLetter(msg.Content[0]) || !char.IsLetter(msg.Content[1])) return;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix("s|", ref argPos))
            {
                //TODO: check for proper channel
                var context = new SocketCommandContext(_client, msg);

                await _commandService.ExecuteAsync(context, argPos, services);
            }
            else if (await _text.TryHaste(msg))
                _ = LoggingHelper.Log(LogSeverity.Verbose, LogSource.Silicon, "Hasted msg");
            else if (IAmRegex.IsMatch(msg.Content))
            {
                var match = IAmRegex.Match(msg.Content);
                _ = msg.Channel.SendMessageAsync($"Hi {match.Groups["name"]}, I'm Silicon!");
            }
        }
        private static readonly Regex IAmRegex = new Regex("(?:i'm|i am|im) (?<name>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private async Task CommandExecuted(Optional<CommandInfo> cmd, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && !result.ErrorReason.IsNullOrWhitespace() && result.ErrorReason != "Unknown command.")
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
                await LoggingHelper.Log(LogSeverity.Warning, LogSource.Module, result.ErrorReason,
                    result is ExecuteResult ? ((ExecuteResult)result).Exception : null);
            }
        }
    }
}