using Discord;
using System.Collections.Generic;

namespace Silicon.Models
{
    public class PaginatedOptions
    {
        public string Name { get; }
        public Color Color { get; }
        public IReadOnlyList<string> Pages { get; }

        public PaginatedOptions(string name, Color color, IReadOnlyList<string> pages)
        {
            Name = name;
            Color = color;
            Pages = pages;
        }
    }
}
