using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;

namespace Espeon.Implementation.Services
{
    [Service(typeof(ITimerService), true)]
    public class TimerService : ITimerService
    {
        private readonly Timer _timer;
        private ConcurrentQueue<TaskObject> _taskQueue;

        private static long _maxTime = (long) (Math.Pow(2, 32) - 2);

        public TimerService()
        {
            _taskQueue = new ConcurrentQueue<TaskObject>();

            _timer = new Timer(async _ =>
            {
                if (!_taskQueue.TryDequeue(out var task)) return;
                await task.RemoveTask(task.Removeable);
                await SetTimerAsync();
            }, null, -1, -1);
        }

        //TODO delayed tasks
        public async Task EnqueueAsync(IRemovable removeable, Func<IRemovable, Task> removeTask)
        {
            var task = new TaskObject
            {
                Removeable = removeable,
                RemoveTask = removeTask
            };

            if(removeable.WhenToRemove > _maxTime)
            {
                task = new TaskObject
                {
                    Removeable = removeable,
                };
            }

            _taskQueue.Enqueue(task);

            await SetTimerAsync();
        }

        public async Task RemoveAsync(IRemovable removable)
        {

            await SetTimerAsync();
        }

        private Task SetTimerAsync()
        {
            if (_taskQueue.IsEmpty)
                return Task.CompletedTask;

            var keepList = new List<TaskObject>();

            foreach (var item in _taskQueue)
            {
                var whenToRemove = DateTimeOffset.FromUnixTimeMilliseconds(item.Removeable.WhenToRemove);

                if(DateTimeOffset.UtcNow - whenToRemove < TimeSpan.FromSeconds(10))
                {
                    Task.Run(async () => await item.RemoveTask(item.Removeable));
                    continue;
                }

                keepList.Add(item);
            }

            _taskQueue = new ConcurrentQueue<TaskObject>(keepList.OrderBy(x => x.Removeable.WhenToRemove));

            var nextTask = _taskQueue.First();
            _timer.Change(nextTask.Removeable.WhenToRemove, -1);

            return Task.CompletedTask;
        }

        private class TaskObject
        {
            public IRemovable Removeable { get; set; }
            public Func<IRemovable, Task> RemoveTask { get; set; }
        }
    }
}
