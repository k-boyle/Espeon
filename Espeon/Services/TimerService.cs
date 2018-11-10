using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Espeon.Core;
using Espeon.Core.Attributes;
using Espeon.Core.Entities;
using Espeon.Core.Services;

namespace Espeon.Services
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

        Task<string> ITimerService.EnqueueAsync(IRemovable removable, Func<string, IRemovable, Task> removeTask)
            => EnqueueAsync(removable, removeTask, true);

        private async Task<string> EnqueueAsync(IRemovable removable, Func<string, IRemovable, Task> removeTask, bool setTimer)
        {
            var key = Random.GenerateKey();
            var task = new TaskObject
            {
                removable = removable,
                RemoveTask = removeTask,
                TaskKey = key
            };

            await EnqueueAsync(task, setTimer);

            return key;
        }

        private Task EnqueueAsync(TaskObject task, bool setTimer)
        {
            if (task.removable.WhenToRemove > MaxTime)
            {
                task = new DelayedTask
                {
                    Task = task
                };
            }

            _taskQueue.Enqueue(task);

            return setTimer ? SetTimerAsync() : Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
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
                var whenToRemove = DateTimeOffset.FromUnixTimeMilliseconds(item.removable.WhenToRemove);

                if (DateTimeOffset.UtcNow - whenToRemove < TimeSpan.FromSeconds(10))
                {
                    Task.Run(() => HandleTaskAsync(item, true));
                    continue;
                }

                keepList.Add(item);
            }

            _taskQueue = new ConcurrentQueue<TaskObject>(keepList.OrderBy(x => x.removable.WhenToRemove));

            var nextTask = _taskQueue.First();
            _timer.Change(nextTask.removable.WhenToRemove, -1);

            return Task.CompletedTask;
        }

        private Task HandleTaskAsync(TaskObject task, bool setTimer)
        {
            if (!(task is DelayedTask delayed))
                return task.RemoveTask(task.TaskKey, task.removable);

            task = delayed.Task;

            return EnqueueAsync(task, setTimer);
        }

        private class TaskObject
        {
            public IRemovable removable { get; set; }
            public Func<string, IRemovable, Task> RemoveTask { get; set; }
            public string TaskKey { get; set; }
        }

        private class DelayedTask : TaskObject
        {
            public TaskObject Task { get; set; }
        }
    }
}
