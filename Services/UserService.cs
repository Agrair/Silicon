﻿using Silicon.Models;
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
            shouldUpdateCount = true;
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

        public User EnsureUser(ulong id, out bool existed)
        {
            existed = true;
            LiteDB.BsonValue docID = null;
            var value = collection.FindOne(x => x.Snowflake == id);
            if (value != null)
            {
                docID = collection.Insert(new User(id));
                collection.EnsureIndex(x => x.Snowflake);
                existed = false;
                shouldUpdateCount = true;
            }
            return existed ? value : collection.FindById(docID);
        }

        private int userCount;
        private bool shouldUpdateCount = true;
        public int UserCount
        {
            get
            {
                if (shouldUpdateCount) { userCount = collection.Count() - 1; shouldUpdateCount = false; }
                return userCount;
            }
        }
    }
}
