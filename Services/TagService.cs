using Discord;
using Discord.WebSocket;
using Silicon.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silicon.Services
{
    public class TagService
    {
        private readonly LiteDB.LiteCollection<Tag> collection;

        public TagService(LiteDB.LiteDatabase db) => collection = db.GetCollection<Tag>("tags");

        public bool TryGetTag(string name, IUser owner, out Tag value)
            => !((value = InternalGetTag(name, owner, true)) is null);

        private readonly char[] IllegalNameLetters =
        {
            '#',
            '@',
            '/',
            '\\',
        };

        public void SetTag(string name, string content, IUser owner, Color embedHue = default)
        {
            name = new string(name.Where(x => !IllegalNameLetters.Contains(x)).ToArray());
            if (InternalGetTag(name, owner) is Tag phrase)
            {
                phrase.Text = content;
                phrase.Color = embedHue == default ? -1 : (int)embedHue.RawValue;
                collection.Update(phrase);
            }
            else
            {
                collection.Insert(new Tag
                {
                    Name = name,
                    Text = content,
                    Owner = owner.Id,
                    Color = embedHue == default ? -1 : (int)embedHue.RawValue,
                    Claimed = true
                });
                collection.EnsureIndex(x => x.Owner);
                collection.EnsureIndex(x => x.Name);
                collection.EnsureIndex(x => x.Text);

                shouldUpdateList = true;
            }
        }

        private Tag InternalGetTag(string name, IUser owner, bool serverWide = false)
        {
            var result = collection.FindOne(x => x.Owner == owner.Id
                && x.Name.EqualsIgnoreCase(name));
            if (serverWide && result == null) result = collection.FindOne(x => x.Name.EqualsIgnoreCase(name));
            return result;
        }

        public bool TryRemoveTag(string name, IUser user) =>
            collection.Delete(x => x.Name == name && x.Owner == user.Id) != 0;

        public bool TryFindTag(string search, out List<Tag> value)
        {
            return (value = collection.Find(x => (x.Name + x.Text).ContainsIgnoreCase(search))
                .ToList()).Count != 0;
        }

        private List<Tag> tags;
        private bool shouldUpdateList = true;
        public List<Tag> Tags
        {
            get
            {
                if (shouldUpdateList) { tags = collection.FindAll().ToList(); shouldUpdateList = false; }
                return tags;
            }
        }

        public void UnclaimPhrases(ulong user)
        {
            foreach (var phrase in collection.Find(x => x.Owner == user))
            {
                phrase.Claimed = false;
                collection.Update(phrase);
            }
            shouldUpdateList = true;
        }

        public bool TryTransfer(SocketUser user, string name, SocketGuildUser newOwner)
        {
            var phrase = InternalGetTag(name, user);
            if (phrase != null)
            {
                phrase.Owner = newOwner.Id;
                collection.Update(phrase);
                return true;
            }
            return false;
        }

        public bool TryClaim(SocketUser user, string name)
        {
            var phrase = InternalGetTag(name, user);
            if (phrase != null && !phrase.Claimed)
            {
                phrase.Owner = user.Id;
                phrase.Claimed = true;
                collection.Update(phrase);
                return true;
            }
            return false;
        }
    }
}
