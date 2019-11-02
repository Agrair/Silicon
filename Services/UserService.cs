using Silicon.Models;
using System;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class UserService
    {
        private readonly LiteDB.LiteCollection<UserConfig> collection;
        private readonly TagService tags;

        public UserService(LiteDB.LiteDatabase db, TagService t)
        {
            collection = db.GetCollection<UserConfig>("users");
            tags = t;
        }

        public void RemoveUser(ulong id)
        {
            collection.Delete(x => x.Snowflake == id);
            tags.UnclaimPhrases(id);
            countUpdated = true;
        }

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

        //private SiliconUser EnsureUser(ulong id) => EnsureUser(id, out _);

        private UserConfig EnsureUser(ulong id, out bool existed)
        {
            existed = true;
            LiteDB.BsonValue docID = null;
            if (!collection.Exists(x => x.Snowflake == id))
            {
                docID = collection.Insert(new UserConfig(id));
                collection.EnsureIndex(x => x.Snowflake);
                existed = false;
                countUpdated = true;
            }
            return existed ? collection.FindOne(x => x.Snowflake == id) : collection.FindById(docID);
        }

        private int userCount;
        private bool countUpdated = true;
        public int UserCount
        {
            get
            {
                if (countUpdated) { userCount = collection.Count() - 1; countUpdated = false; }
                return userCount;
            }
        }
    }
}
