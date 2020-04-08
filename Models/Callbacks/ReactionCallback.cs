using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Silicon.Models.Callbacks
{
    public abstract class ReactionCallback
    {
        public IUserMessage Message { get; protected set; }

        public SocketCommandContext FirstContext { get; }

        public bool Async => false;

        protected ReactionCallback(SocketCommandContext context)
        {
            FirstContext = context;
        }

        public virtual bool Judge(SocketReaction reaction) =>
            FirstContext.User.Id == reaction.UserId && FirstContext.Channel.Id == reaction.Channel.Id;

        public abstract Task<bool> ExecuteAsync(SocketReaction reaction);
    }
}
