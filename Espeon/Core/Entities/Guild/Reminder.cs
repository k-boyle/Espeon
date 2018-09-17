using System;
using System.Threading.Tasks;
using Espeon.Interfaces;

namespace Espeon.Core.Entities.Guild
{
    public class Reminder : IRemoveable
    {
        private readonly IRemoveableService _service;

        public Reminder(IRemoveableService service)
        {
            _service = service;
        }

        public Reminder() { }

        public string TheReminder { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }

        public int Identifier { get; set; }
        public DateTime When { get; set; }

        public Task RemoveAsync()
            => _service.RemoveAsync(this);
    }
}
