using System;
using System.Threading.Tasks;
using Espeon.Interfaces;

namespace Espeon.Core.Entities
{
    public class Message : IRemoveable
    {
        private readonly IRemoveableService _service;

        public Message(IRemoveableService service)
        {
            _service = service;
        }

        public Message() { }

        public ulong UserId { get; set; }
        public ulong ExecutingId { get; set; }
        public ulong ResponseId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool AttachedFile { get; set; }

        public int Identifier { get; set; }
        public DateTime When => DateTime.UtcNow + TimeSpan.FromMinutes(5);

        public Task RemoveAsync()
            => _service.RemoveAsync(this);
    }
}
