using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class TimerService : BaseService
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
                await HandleTaskAsync(task, false);
                await SetTimerAsync();
            }, null, -1, -1);
        }

        public Task<string> EnqueueAsync(object removable, long whenToRemove, Func<string, object, Task> removeTask)
            => EnqueueAsync(removable, whenToRemove, removeTask, true);

        private async Task<string> EnqueueAsync(object removable, long whenToRemove, Func<string, object, Task> removeTask, bool setTimer)
        {
            var key = Guid.NewGuid().ToString();
            var task = new TaskObject
            {
                Removable = removable,
                WhenToRemove = whenToRemove,
                RemoveTask = removeTask,
                TaskKey = key
            };

            await EnqueueAsync(task, setTimer);

            return key;
        }

        private Task EnqueueAsync(TaskObject task, bool setTimer)
        {
            if (task.WhenToRemove > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + MaxTime)
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
                var whenToRemove = DateTimeOffset.FromUnixTimeMilliseconds(item is DelayedTask ? MaxTime : item.WhenToRemove);

                if (whenToRemove - DateTimeOffset.UtcNow < TimeSpan.FromSeconds(10))
                {
                    Task.Run(() => HandleTaskAsync(item, true));
                    continue;
                }

                keepList.Add(item);
            }

            _taskQueue = new ConcurrentQueue<TaskObject>(keepList.OrderBy(x => x.WhenToRemove));

            if (_taskQueue.IsEmpty)
                return Task.CompletedTask;

            var nextTask = _taskQueue.First();
            _timer.Change(nextTask.WhenToRemove - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), -1);

            return Task.CompletedTask;
        }

        private Task HandleTaskAsync(TaskObject task, bool setTimer)
        {
            if (!(task is DelayedTask delayed))
                return task.RemoveTask(task.TaskKey, task.Removable);

            task = delayed.Task;

            return EnqueueAsync(task, setTimer);
        }

        private class TaskObject
        {
            public object Removable { get; set; }
            public long WhenToRemove { get; set; }
            public Func<string, object, Task> RemoveTask { get; set; }
            public string TaskKey { get; set; }
        }

        private class DelayedTask : TaskObject
        {
            public TaskObject Task { get; set; }
        }
    }
}
