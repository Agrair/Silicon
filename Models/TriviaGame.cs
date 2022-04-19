using Discord.WebSocket;
using System;
using System.Threading;

namespace Silicon.Models
{
    public class TriviaGame
    {
        public SocketTextChannel Channel { get; set; }

        public Timer Timer { get; set; }

        public int Correct { get; set; }

        public string Title { get; set; }

        public string[] Choices { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
