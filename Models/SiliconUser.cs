using System;
using System.Collections.Generic;

namespace Silicon.Models
{
    public class SiliconUser
    {
        public SiliconUser(ulong id)
        {
            Snowflake = id;
            TimeSlots = new Dictionary<string, DateTimeOffset>();
        }

        public int Id { get; set; }

        public ulong Snowflake { get; set; }

        public Dictionary<string, DateTimeOffset> TimeSlots { get; set; }
    }
}
