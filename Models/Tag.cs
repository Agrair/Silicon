namespace Silicon.Models
{
    public class Tag : LiteDBPoco
    {
        public string Name { get; set; }

        public string Text { get; set; }

        public ulong Owner { get; set; }

        public bool Claimed { get; set; }

        public int Color { get; set; }
    }
}
