using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Silicon.Models
{
    public class PollData
    {
        public string Name { get; set; }

        public string[] Options { get; set; }

        public HashSet<ulong>[] Votes { get; set; }


        public PollData(string name, string[] options)
        {
            Name = name;
            Options = options;
            Votes = new HashSet<ulong>[options.Length];
        }
    }
}
