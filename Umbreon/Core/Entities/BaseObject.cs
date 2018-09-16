using LiteDB;
using System;
using System.Threading.Tasks;
using Umbreon.Interfaces;

namespace Umbreon.Core.Entities
{
    public abstract class BaseObject : IRemoveable
    {
        private readonly IRemoveableService _service;

        protected BaseObject(BaseObject baseObj)
        {
            _service = baseObj._service;
            Id = baseObj.Id;
            Identifier = baseObj.Identifier;
            When = baseObj.When;
        }

        protected BaseObject(IRemoveableService service)
        {
            _service = service;
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
