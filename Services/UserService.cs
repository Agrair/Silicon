using Silicon.Models;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class UserService
    {
        private readonly LiteDB.LiteCollection<SiliconUser> collection;
        private readonly TagService phrases;

        public UserService(LiteDB.LiteDatabase db, TagService p)
        {
            collection = db.GetCollection<SiliconUser>("users");
            phrases = p;
        }

        public Task RemoveUser(ulong id)
        {
            collection.Delete(x => x.Snowflake == id);
            phrases.UnclaimPhrases(id);
            countUpdated = true;
            return Task.CompletedTask;
        }

        /*
        private PandoraUser GetUser(ulong id)
        {
            if (!collection.Exists(x => x.Snowflake == id))
            {
                collection.Insert(new PandoraUser(id));
            }
            return collection.FindOne(x => x.Snowflake == id);
        }
        */

        private int userCount;
        private bool countUpdated = false;
        public int GetUserCount()
        {
            if (!countUpdated) { userCount = collection.Count() - 1; countUpdated = true; }
            return userCount;
        }
    }
}
