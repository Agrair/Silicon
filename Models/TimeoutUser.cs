using System;

namespace Silicon.Models
{
    public class TimeoutUser
    {
        public static readonly TimeSpan TwoSeconds = TimeSpan.FromSeconds(2);

        public DateTime start;
        public TimeSpan expire;
        public ushort count;

        public TimeoutUser()
        {
            start = DateTime.Now;
            expire = TwoSeconds;
            count = 1;
        }
    }
}
