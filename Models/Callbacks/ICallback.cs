using Discord.Commands;
using Silicon.Services;

namespace Silicon.Models.Callbacks
{
    public interface ICallback
    {
        public SocketCommandContext FirstContext { get; }
        public bool Async { get; }
        public InteractiveService ManagerService { get; }
    }
}
