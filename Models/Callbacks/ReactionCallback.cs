using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Models.Callbacks
{
    public abstract class ReactionCallback : ICallback
    {
        public IUserMessage Message { get; protected set; }

        public SocketCommandContext FirstContext { get; }

        public bool Async => false;

        public InteractiveService ManagerService { get; }

        public ReactionCallback(SocketCommandContext context, InteractiveService reaction)
        {
            FirstContext = context;
            ManagerService = reaction;
        }

        public virtual Task<bool> JudgeAsync(SocketReaction reaction) => Task.FromResult(FirstContext.User.Id == reaction.UserId);

        public abstract Task<bool> ExecuteAsync(SocketReaction reaction);
    }
}
