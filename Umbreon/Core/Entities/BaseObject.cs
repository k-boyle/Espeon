using System;
using LiteDB;
using Umbreon.Interfaces;

namespace Umbreon.Core.Entities
{
    public class BaseObject : IRemoveable
    {
        [BsonId(false)]
        public ulong Id { get; set; }

        public int Identifier { get; set; }
        public DateTime When { get; set; }
        public IRemoveableService Service { get; set; }
    }
}
