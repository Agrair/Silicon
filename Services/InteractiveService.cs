using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Models;
using Silicon.Models.Callbacks;
using System;
using System.Collections.Generic;
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

        public async Task<SocketMessage> GetResponseAsync(DiscordSocketClient client,
            TimeSpan timeout,
            Func<SocketMessage, bool> judge)
        {
            var eventTrigger = new TaskCompletionSource<SocketMessage>();

            Task Handler(SocketMessage message)
            {
                if (judge(message))
                    eventTrigger.SetResult(message);
                return Task.CompletedTask;
            }

            client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var task = await Task.WhenAny(trigger, Task.Delay(timeout));

            client.MessageReceived -= Handler;

            return task == trigger ? await trigger : null;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(SocketCommandContext context,
            PaginationData pager)
        {
            var callback = new PaginatedCallback(context, pager);
            await callback.SendAsync();
            AddReactionCallback(callback);
            return callback.Message;
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id)
                return;
            if (!reactions.TryGetValue(message.Id, out var callback))
                return;
            if (!callback.Judge(reaction))
                return;

            if (await callback.ExecuteAsync(reaction))
                RemoveReactionCallback(message.Id);
        }
    }
}
