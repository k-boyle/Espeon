using LiteDB;
using System;
using Umbreon.Interfaces;

namespace Umbreon.Core.Entities
{
    public abstract class BaseObject : IRemoveable
    {
        [BsonId(false)]
        public ulong Id { get; set; }

        public int Identifier { get; set; }
        public DateTime When { get; set; }
        public IRemoveableService Service { get; set; }
    }
}
