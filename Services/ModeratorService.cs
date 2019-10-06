using Discord.WebSocket;
using Silicon.Models;

namespace Silicon.Services
{
    public class ModeratorService
    {
        private readonly LiteDB.LiteCollection<SiliconChannel> _channels;

        public ModeratorService(LiteDB.LiteDatabase db)
        {
            _channels = db.GetCollection<SiliconChannel>("channels");
        }

        public bool IsBotChannel(ulong id) => _channels.Exists(x => x.Snowflake == id);

        public void AddBotChannel(SocketGuildChannel channel)
        {
            if (_channels.Exists(x => x.Snowflake == channel.Id)) return;
            _channels.Insert(channel.ToDatabaseValue());
            _channels.EnsureIndex(x => x.Snowflake, true);
        }

        public void RemoveBotChannel(SocketGuildChannel channel)
        {
            _channels.Delete(x => x.Snowflake == channel.Id);
        }
    }
}
