using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Silicon.Commands.Commons
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    internal sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint invokeLimit;
        private readonly TimeSpan invokePeriod;
        private readonly Dictionary<ulong, CommandTimeout> tracker = new Dictionary<ulong, CommandTimeout>();

        public RatelimitAttribute(
            uint times,
            double period)
        {
            invokeLimit = times;

            invokePeriod = TimeSpan.FromSeconds(period);
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context,
            CommandInfo command,
            IServiceProvider services)
        {
            var result = PreconditionResult.FromSuccess();

            var now = DateTimeOffset.UtcNow;
            var key = context.User.Id;

            var timeout = (tracker.TryGetValue(key, out var t)
                && ((now - t.FirstInvoke) < invokePeriod))
                    ? t : new CommandTimeout(now);

            if (++timeout.TimesInvoked <= invokeLimit)
            {
                tracker[key] = timeout;
            }
            else
            {
                result = PreconditionResult.FromError($"You're going too fast!");
            }
            return Task.FromResult(result);
        }

        private sealed class CommandTimeout
        {
            public uint TimesInvoked { get; set; }
            public DateTimeOffset FirstInvoke { get; }

            public CommandTimeout(DateTimeOffset timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }
}