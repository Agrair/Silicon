using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silicon.Commands.Commons
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private readonly uint invokeLimit;
        private readonly TimeSpan invokePeriod;
        private readonly ConcurrentDictionary<ulong, CommandTimeout> tracker = new ConcurrentDictionary<ulong, CommandTimeout>();
        //TODO: move to a faster model like skip list
        private readonly List<ulong> warned = new List<ulong>();

        public RatelimitAttribute(uint times, double period)
        {
            invokeLimit = times;

            invokePeriod = TimeSpan.FromSeconds(period);
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command,
            IServiceProvider services)
        {
            var result = PreconditionResult.FromSuccess();

            var now = DateTimeOffset.UtcNow;
            var user = context.User.Id;

            if (!tracker.TryGetValue(user, out CommandTimeout timeout) || (now - timeout.FirstInvoke) >= invokePeriod)
            {
                timeout = new CommandTimeout(now);
                warned.Remove(user);
            }

            if (++timeout.TimesInvoked <= invokeLimit)
            {
                tracker[user] = timeout;
            }
            else if (!warned.Contains(user))
            {
                result = PreconditionResult.FromError($"You're going too fast!");
                warned.Add(user);
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