namespace Silicon.Models
{
    public class SiliconUser
    {
        public SiliconUser(ulong id)
        {
            Snowflake = id;
        }

        public int Id { get; set; }

        public ulong Snowflake { get; set; }
    }
}
