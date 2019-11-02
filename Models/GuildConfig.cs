using System.Collections.Generic;

namespace Silicon.Models
{
    //TODO
    public class GuildConfig
    {
        public int Id { get; set; }

        public ulong Snowflake { get; set; }

        public string Prefix { get; set; } = "s:";

        public List<string> DisabledModules { get; set; } = new List<string>();
    }
}
