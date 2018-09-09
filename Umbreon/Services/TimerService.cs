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
        private readonly IServiceProvider _services;

        private Timer _timer;
        private ConcurrentQueue<IRemoveable> _queue = new ConcurrentQueue<IRemoveable>();

        public TimerService(IServiceProvider services)
        {
            _services = services;
        }

        public void InitialiseTimer()
        {
            _timer = new Timer(_ =>
            {
                if (!_queue.TryDequeue(out var removeable)) return;
                HandleRemoveableAsync(removeable);
                SetTimer();
            }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Enqueue(IRemoveable removeable)
        {
            _queue.Enqueue(removeable);
            _queue = new ConcurrentQueue<IRemoveable>(_queue.OrderBy(x => x.When));
            if (!_queue.TryPeek(out _)) return;
            SetTimer();
        }

        private void SetTimer()
        {
            IRemoveable removeable;
            while (_queue.TryDequeue(out removeable) && removeable.When - DateTime.UtcNow < TimeSpan.Zero)
            {
                HandleRemoveableAsync(removeable);
            }

            _timer.Change(removeable.When - DateTime.UtcNow, TimeSpan.FromMilliseconds(-1));
        }

        private void HandleRemoveableAsync(IRemoveable removeable)
            => _ = Task.Run(async () =>
             {
                 var service = _services.GetService(removeable.Service.GetType());
                 if (service is IRemoveableService removeableService)
                     await removeableService.RemoveAsync(removeable);
             });

        private void Remove(IRemoveable obj)
        {
            var newQueue = new ConcurrentQueue<IRemoveable>();
            foreach (var item in _queue)
            {
                if (item.Identifier == obj.Identifier) continue;
                newQueue.Enqueue(item);
            }

            _queue = newQueue;
        }

        public void Remove(IEnumerable<IRemoveable> objs)
        {
            var newCol = _queue.Except(objs);
            _queue = new ConcurrentQueue<IRemoveable>(newCol);
        }

        public void Update(IRemoveable removeable)
        {
            Remove(removeable);
            Enqueue(removeable);
        }
    }
}
