using Discord.Commands;

namespace Silicon.Models.Callbacks
{
    public interface ICallback
    {
        public SocketCommandContext FirstContext { get; }
        public bool Async { get; }
    }
}
