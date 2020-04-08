using System;
using System.IO;

namespace Silicon
{
    public static class Program
    {
        public static readonly Random rand = new Random();
        public static readonly string ready = File.ReadAllText(@"ready.txt");
        public static ulong userID = 588515073396310036;
        public static string avatar = "https://" +
            "cdn.discordapp.com/avatars/588515073396310036/" +
            "e21fb266ba7c9bc07304af5d9f24dea1.png";

        public static void Main() => Core.SiliconBot.StartAsync().GetAwaiter().GetResult();
    }
}
