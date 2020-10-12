using Discord.WebSocket;
using System.Threading;

namespace Silicon.Models
{
    public class TriviaGame
    {
        public SocketGuildChannel Channel { get; set; }

        public Timer Timer { get; }

        public (SocketGuildChannel, Timer) AsTuple => (Channel, Timer);

        public TriviaGame(SocketGuildChannel channel, Timer timer)
        {
            Channel = channel;
            Timer = timer;
        }
    }
}
