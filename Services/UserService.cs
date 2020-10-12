using Silicon.Models;
using System;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class UserService
    {
        private readonly LiteDB.LiteCollection<User> collection;
        private readonly TagService tags;

        public UserService(LiteDB.LiteDatabase db, TagService t)
        {
            collection = db.GetCollection<User>("users");
            tags = t;
        }

        public void RemoveUser(ulong id)
        {
            collection.Delete(x => x.Snowflake == id);
            tags.UnclaimPhrases(id);
        }

        /*
        public bool LockTimeslot(ulong id,
            TimeSpan cooldown,
            string timeslot,
            out DateTimeOffset end)
        {
            end = default;
            var user = EnsureUser(id, out bool existed);
            bool result = false;
            if (existed && user.TimeSlots.TryGetValue(timeslot, out var start))
            {
                end = start.Add(cooldown);
                result = true;
                _ = Task.Delay(cooldown).ContinueWith(_ =>
                {
                    user.TimeSlots.Remove(timeslot);
                    collection.Update(user);
                });
            }
            else
            {
                user.TimeSlots.Add(timeslot, DateTimeOffset.UtcNow);
            }
            collection.Update(user);
            return result;
        }
        */

        public User EnsureUser(ulong id, out bool existed)
        {
            if (collection.FindOne(x => x.Snowflake == id) is User value)
            {
                existed = false;
                return value;
            }

            collection.Insert(value = new User(id));
            collection.EnsureIndex(x => x.Snowflake);

            existed = false;
            return value;
        }

        public bool TryGetUser(ulong id, out User user)
        {
            return (user = collection.FindOne(x => x.Snowflake == id)) != null;
        }

        public void AddUser(ulong id)
        {
            collection.Insert(new User(id));
            collection.EnsureIndex(x => x.Snowflake, true);
        }
    }
}
