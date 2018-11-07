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
        private readonly Timer _timer;
        private ConcurrentQueue<TaskObject> _taskQueue;

        private static readonly long MaxTime = (long)(Math.Pow(2, 32) - 2);

        public TimerService()
        {
            _taskQueue = new ConcurrentQueue<TaskObject>();

            _timer = new Timer(async _ =>
            {
                if (!_taskQueue.TryDequeue(out var task)) return;
                await HandleTaskAsync(task);
                await SetTimerAsync();
            }, null, -1, -1);
        }

        Task ITimerService.EnqueueAsync(IRemovable removable, Func<IRemovable, Task> removeTask)
            => EnqueueAsync(removable, removeTask, true);

        private Task EnqueueAsync(IRemovable removeable, Func<IRemovable, Task> removeTask, bool setTimer)
        {
            var task = new TaskObject
            {
                Removeable = removeable,
                RemoveTask = removeTask
            };

            if (removeable.WhenToRemove > MaxTime)
            {
                task = new DelayedTask
                {
                    Task = task
                };
            }

            _taskQueue.Enqueue(task);

            return setTimer ? SetTimerAsync() : Task.CompletedTask;
        }

        public Task RemoveAsync(IRemovable removable)
        {
            var removed = _taskQueue.Where(x => x.Removeable.TaskKey != removable.TaskKey);
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
                    Task.Run(() => HandleTaskAsync(item));
                    continue;
                }

                keepList.Add(item);
            }

            _taskQueue = new ConcurrentQueue<TaskObject>(keepList.OrderBy(x => x.Removeable.WhenToRemove));

            var nextTask = _taskQueue.First();
            _timer.Change(nextTask.Removeable.WhenToRemove, -1);

            return Task.CompletedTask;
        }

        private Task HandleTaskAsync(TaskObject task)
        {
            if (!(task is DelayedTask delayed)) return task.RemoveTask(task.Removeable);
            task = delayed.Task;

            return EnqueueAsync(task.Removeable, task.RemoveTask, false);
        }

        private class TaskObject
        {
            public IRemovable Removeable { get; set; }
            public Func<IRemovable, Task> RemoveTask { get; set; }
        }

        private class DelayedTask : TaskObject
        {
            public TaskObject Task { get; set; }
        }
    }
}
