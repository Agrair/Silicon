using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Models;
using Silicon.Models.Callbacks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Silicon.Services
{
    //https://github.com/foxbot/Discord.Addons.Interactive/blob/master/Discord.Addons.Interactive/InteractiveService.cs
    public class InteractiveService
    {
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<ulong, ReactionCallback> reactions;

        public InteractiveService(DiscordSocketClient discord)
        {
            _client = discord;
            _client.ReactionAdded += HandleReactionAsync;

            reactions = new Dictionary<ulong, ReactionCallback>();
        }

        public void AddReactionCallback(ReactionCallback callback)
            => reactions[callback.Message.Id] = callback;
        public void RemoveReactionCallback(ulong msg)
            => reactions.Remove(msg);

        public async Task<SocketMessage> GetResponseAsync(SocketCommandContext context,
            TimeSpan timeout,
            Func<SocketMessage, bool> judge,
            CancellationToken token)
        {
            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            Task Handler(SocketMessage message)
            {
                if (judge(message)) eventTrigger.SetResult(message);
                return Task.CompletedTask;
            }

            context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var task = await Task.WhenAny(trigger, Task.Delay(timeout), cancelTrigger.Task);

            context.Client.MessageReceived -= Handler;

            return task == trigger ? await trigger : null;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(SocketCommandContext context,
            PaginatedOptions pager)
        {
            var callback = new PaginatedCallback(context, this, pager);
            await callback.SendAsync();
            AddReactionCallback(callback);
            return callback.Message;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;
            if (!reactions.TryGetValue(message.Id, out var callback)) return;
            if (!await callback.JudgeAsync(reaction)) return;
            if (callback.Async)
            {
                _ = Task.Run(async () =>
                {
                    if (await callback.ExecuteAsync(reaction))
                        RemoveReactionCallback(message.Id);
                });
            }
            else
            {
                if (await callback.ExecuteAsync(reaction))
                    RemoveReactionCallback(message.Id);
            }
        }
    }
}
