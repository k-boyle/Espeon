using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Interfaces;

namespace Espeon.Services
{
    [Service]
    public class TimerService
    {
        private readonly Timer _timer;

        private static TimeSpan MaxTime =>
            TimeSpan.FromMilliseconds(Math.Pow(2, 32) - 2);

        private ConcurrentQueue<IRemoveable> _queue = new ConcurrentQueue<IRemoveable>();

        public TimerService()
        {
            _timer = new Timer(async _ =>
                {
                    try
                    {
                        if (!_queue.TryDequeue(out var removeable)) return;
                        await HandleRemoveableAsync(removeable);
                        SetTimer();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }, null,
                TimeSpan.FromMilliseconds(-1),
                TimeSpan.FromMilliseconds(-1));
        }

        public void Enqueue(IRemoveable removeable)
        {
            if (removeable is DelayedRemovable delayed)
                removeable = delayed.Removeable;

            //Timer can't have has a value greater than TimeSpan.FromMilliseconds(Math.Pow(2,32) - 2)
            if (removeable.When - DateTime.UtcNow > MaxTime)
                removeable = new DelayedRemovable(removeable);

            _queue.Enqueue(removeable);
            SetTimer();
        }

        private void SetTimer()
        {
            try
            {
                if (_queue.IsEmpty) return;

                //Added some overhead to try avoid the very small chance of a race condition
                var toRemove = _queue.Where(x =>
                    x.When.ToUniversalTime() - DateTime.UtcNow <
                    TimeSpan.FromSeconds(10)).ToArray();

                _queue = new ConcurrentQueue<IRemoveable>(_queue
                    .Where(x => x.When.ToUniversalTime() - DateTime.UtcNow > TimeSpan.FromSeconds(10))
                    .OrderBy(x => x.When));

                if (toRemove.Length > 0)
                {
                    //Stops potential race condition
                    _ = Task.Run(async () =>
                    {
                        foreach (var item in toRemove)
                            await item.RemoveAsync();
                    });
                }

                if (_queue.TryPeek(out var removeable))
                    _timer.Change(removeable.When.ToUniversalTime() - DateTime.UtcNow, TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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

        public void RemoveRange(IEnumerable<IRemoveable> objs)
        {
            var newCol = _queue.Except(objs);
            _queue = new ConcurrentQueue<IRemoveable>(newCol);
            SetTimer();
        }

        public async Task UpdateAsync(IRemoveable removeable)
        {
            await RemoveAsync(removeable);
            Enqueue(removeable);
            SetTimer();
        }

        private class DelayedRemovable : IRemoveable
        {
            public IRemoveable Removeable { get; }

            public int Identifier => throw new NotImplementedException();
            public DateTime When { get; }

            public DelayedRemovable(IRemoveable removeable)
            {
                Removeable = removeable;
                When = DateTime.UtcNow.Add(MaxTime);
            }

            public Task RemoveAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
