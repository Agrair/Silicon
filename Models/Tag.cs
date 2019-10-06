namespace Silicon.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public ulong Owner { get; set; }

        public bool Claimed { get; set; }

        public int Color { get; set; }
    }
}
