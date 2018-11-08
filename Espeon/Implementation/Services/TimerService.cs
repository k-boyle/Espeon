using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Implementation.Services
{
    [Service(typeof(ITimerService), true)]
    public class TimerService : ITimerService
    {
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        private readonly Timer _timer;
        private ConcurrentQueue<TaskObject> _taskQueue;

        private static readonly long MaxTime = (long)(Math.Pow(2, 32) - 2);

        public TimerService()
        {
            _taskQueue = new ConcurrentQueue<TaskObject>();

            _timer = new Timer(async _ =>
            {
                if (!_taskQueue.TryDequeue(out var task)) return;
                await HandleTaskAsync(task, false);
                await SetTimerAsync();
            }, null, -1, -1);
        }

        Task<int> ITimerService.EnqueueAsync(IRemovable removable, Func<IRemovable, Task> removeTask)
            => EnqueueAsync(removable, removeTask, true);

        private async Task<int> EnqueueAsync(IRemovable removeable, Func<IRemovable, Task> removeTask, bool setTimer)
        {
            var key = Random.Next();
            var task = new TaskObject
            {
                Removeable = removeable,
                RemoveTask = removeTask,
                TaskKey = key
            };

            await EnqueueAsync(task, setTimer);

            return key;
        }

        private Task EnqueueAsync(TaskObject task, bool setTimer)
        {
            if (task.Removeable.WhenToRemove > MaxTime)
            {
                task = new DelayedTask
                {
                    Task = task
                };
            }

            _taskQueue.Enqueue(task);

            return setTimer ? SetTimerAsync() : Task.CompletedTask;
        }

        public Task RemoveAsync(int key)
        {
            var removed = _taskQueue.Where(x => x.TaskKey != key);
            _taskQueue = new ConcurrentQueue<TaskObject>(removed);

            return SetTimerAsync();
        }

        private Task SetTimerAsync()
        {
            if (_taskQueue.IsEmpty)
                return Task.CompletedTask;

            var keepList = new List<TaskObject>();

            foreach (var item in _taskQueue)
            {
                var whenToRemove = DateTimeOffset.FromUnixTimeMilliseconds(item.Removeable.WhenToRemove);

                if (DateTimeOffset.UtcNow - whenToRemove < TimeSpan.FromSeconds(10))
                {
                    Task.Run(() => HandleTaskAsync(item, true));
                    continue;
                }

                keepList.Add(item);
            }

            _taskQueue = new ConcurrentQueue<TaskObject>(keepList.OrderBy(x => x.Removeable.WhenToRemove));

            var nextTask = _taskQueue.First();
            _timer.Change(nextTask.Removeable.WhenToRemove, -1);

            return Task.CompletedTask;
        }

        private Task HandleTaskAsync(TaskObject task, bool setTimer)
        {
            if (!(task is DelayedTask delayed))
                return task.RemoveTask(task.Removeable);

            task = delayed.Task;

            return EnqueueAsync(task, setTimer);
        }

        private class TaskObject
        {
            public IRemovable Removeable { get; set; }
            public Func<IRemovable, Task> RemoveTask { get; set; }
            public int TaskKey { get; set; }
        }

        private class DelayedTask : TaskObject
        {
            public TaskObject Task { get; set; }
        }
    }
}
