namespace Silicon.Models
{
    public class User : LiteDBPoco
    {
        public User(ulong id)
        {
            Snowflake = id;
            //TimeSlots = new Dictionary<string, DateTimeOffset>();
        }

        public ulong Snowflake { get; set; }

        //public Dictionary<string, DateTimeOffset> TimeSlots { get; set; }
    }
}
