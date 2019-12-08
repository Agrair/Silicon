using System.Collections.Generic;

namespace Silicon.Models
{
    public class GuildConfig
    {
        public int Id { get; set; }

        public ulong Snowflake { get; set; }

        public string Prefix { get; set; } = "|";

        public List<string> DisabledModules { get; set; } = new List<string>();
    }
}
