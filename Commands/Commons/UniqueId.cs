using System;

namespace Silicon.Commands.Commons
{
    internal class UniqueNameAttribute : Attribute
    {
        public string id;

        public UniqueNameAttribute(string id) => this.id = id;
    }
}
