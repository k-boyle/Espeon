using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Interfaces;

namespace Umbreon.Services
{
    [Service]
    public class TimerService
    {
        private readonly Timer _timer;

        private ConcurrentQueue<IRemoveable> _queue = new ConcurrentQueue<IRemoveable>();
        
        public TimerService()
        {
            _timer = new Timer(async _ =>
                {
                    if (!_queue.TryDequeue(out var removeable)) return;
                    await HandleRemoveableAsync(removeable);
                    await SetTimerAsync();
                }, null,
                TimeSpan.FromMilliseconds(-1),
                TimeSpan.FromMilliseconds(-1));
        }

        public async Task EnqueueAsync(IRemoveable removeable)
        {
            _queue.Enqueue(removeable);
            await SetTimerAsync();
        }

        private async Task SetTimerAsync()
        {
            _queue = new ConcurrentQueue<IRemoveable>(_queue.OrderBy(x => x.When));

            IRemoveable removeable;
            while (_queue.TryPeek(out removeable))
            {
                if (!(removeable.When.ToUniversalTime() - DateTime.UtcNow < TimeSpan.Zero)) break;
                if(!_queue.TryDequeue(out removeable)) continue;
                await HandleRemoveableAsync(removeable);
            }

            _timer.Change(removeable.When.ToUniversalTime() - DateTime.UtcNow, TimeSpan.FromMilliseconds(-1));
        }

        private static Task HandleRemoveableAsync(IRemoveable removeable)
            => removeable.RemoveAsync();

        private Task RemoveAsync(IRemoveable obj)
        {
            var newQueue = new ConcurrentQueue<IRemoveable>();
            foreach (var item in _queue)
            {
                if (item.Identifier == obj.Identifier) continue;
                newQueue.Enqueue(item);
            }

            _queue = newQueue;
            return Task.CompletedTask;
        }

        public async Task RemoveRangeAsync(IEnumerable<IRemoveable> objs)
        {
            var newCol = _queue.Except(objs);
            _queue = new ConcurrentQueue<IRemoveable>(newCol);
            await SetTimerAsync();
        }

        public async Task UpdateAsync(IRemoveable removeable)
        {
            await RemoveAsync(removeable);
            await EnqueueAsync(removeable);
            await SetTimerAsync();
        }
    }
}
