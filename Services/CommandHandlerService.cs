using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Helpers;
using Silicon.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class CommandHandlerService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private readonly Dictionary<ulong, TimeoutUser> timeouts;

        public CommandHandlerService(IServiceProvider services)
        {
            _services = services;
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            timeouts = new Dictionary<ulong, TimeoutUser>();
        }

        public async Task HandleCmdAsync(SocketUserMessage msg, int argPos)
        {
            if (UserTimeout(msg.Author.Id, out string reply))
            {
                if (reply != null) await msg.Channel.SendMessageAsync(reply);
                return;
            }

            var context = new Commands.PandoraContext(_client, msg);

            await _commandService.ExecuteAsync(context, argPos, _services);
        }

        private bool UserTimeout(ulong user, out string msg)
        {
            msg = null;
            if (timeouts.TryGetValue(user, out var value))
            {
                if (value.start.Add(value.expire).CompareTo(DateTime.Now) != 1)
                {
                    timeouts.Remove(user);
                    return false;
                }
                var diff = value.start.Add(TimeoutUser.TwoSeconds * value.count++).Subtract(DateTime.Now);
                value.expire = (TimeoutUser.TwoSeconds * value.count).Add(diff);
                timeouts[user] = value;
                msg = value.count == 2 ? new[] { "Slow down!", "Too fast", "Plz no spam", "Too many commands" }.NextIndex() : null;
                return true;
            }
            else
            {
                timeouts[user] = new TimeoutUser();
                return false;
            }
        }

        public static async Task CommandExecuted(ICommandContext context, IResult result)
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
