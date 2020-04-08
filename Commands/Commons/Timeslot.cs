/*
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Silicon.Services;
using System;
using System.Threading.Tasks;

namespace Silicon.Commands.Commons
{
    public sealed class Timeslot : PreconditionAttribute
    {
        private readonly TimeSpan time;
        private readonly string timeslot;
        private readonly string format;

        public Timeslot(uint time,
            string timeslot,
            string format = "You've already used your {0} timeslot for the next {1}!")
        {
            this.time = TimeSpan.FromSeconds(time);

            this.timeslot = timeslot;
            this.format = format;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var db = services.GetRequiredService<UserService>();
            var result = PreconditionResult.FromSuccess();

            if (db.LockTimeslot(context.User.Id, time, timeslot, out var end))
            {
                result = PreconditionResult.FromError(
                    string.Format(format, timeslot, end.Subtract(DateTimeOffset.UtcNow).Normalize()));
            }
            return Task.FromResult(result);
        }
    }
}
*/