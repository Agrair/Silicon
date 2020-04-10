using System;
using System.Collections.Generic;

namespace Silicon.Models
{
    public class User
    {
        public User(ulong id)
        {
            Snowflake = id;
        }

        public int Id { get; set; }

        public ulong Snowflake { get; set; }

        public Dictionary<string, DateTimeOffset> TimeSlots { get; set; } = new Dictionary<string, DateTimeOffset>();
    }
}
