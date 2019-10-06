using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Services;
using System;
using System.Threading.Tasks;

namespace Silicon.Modules.Commons
{
    internal class BotChannelsOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var result = services.GetRequiredService<ModeratorService>().IsBotChannel(context.Channel.Id)
                || (context.Channel is Discord.IPrivateChannel);
            return Task.FromResult(result ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(""));
        }
    }
}