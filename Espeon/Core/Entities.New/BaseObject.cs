using System;
using System.Threading.Tasks;
using Espeon.Interfaces;
using LiteDB;

namespace Espeon.Core.Entities.New
{
    public abstract class BaseObject : IRemoveable
    {
        private readonly IRemoveableService _service;

        protected BaseObject(BaseObject baseObj, IRemoveableService service)
        {
            _service = service;
            Id = baseObj.Id;
            Identifier = baseObj.Identifier;
            When = baseObj.When;
        }

        protected BaseObject() { }

        [BsonId(false)]
        public ulong Id { get; set; }

        public int Identifier { get; set; }
        public DateTime When { get; set; }

        public Task RemoveAsync()
            => _service.RemoveAsync(this);
    }
}
