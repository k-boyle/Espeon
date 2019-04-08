using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon
{
    public class TaskScheduler
    {
        private ConcurrentQueue<ScheduledTask> _taskQueue;
        private CancellationTokenSource _cts;

        private TaskScheduler()
        {
            _taskQueue = new ConcurrentQueue<ScheduledTask>();
            _cts = new CancellationTokenSource();
        }

        public static TaskScheduler Create()
        {
            var scheduler = new TaskScheduler();

            _ = scheduler.HandleCallsbackAsync();

            return scheduler;
        }

        public Guid ScheduleTask(object obj, long whenToRemove, Func<Guid, object, Task> task)
        {
            var key = Guid.NewGuid();

            var toAdd = new ScheduledTask
            {
                Object = obj,
                WhenToRemove = whenToRemove,
                Task = task,
                Key = key
            };

            _taskQueue.Enqueue(toAdd);
            _taskQueue = new ConcurrentQueue<ScheduledTask>(_taskQueue.OrderBy(x => x.WhenToRemove));

            _cts.Cancel(true);

            return key;
        }

        public void CancelTask(Guid key)
        {
            var removed = _taskQueue.Where(x => x.Key != key).OrderBy(x => x.WhenToRemove);
            _taskQueue = new ConcurrentQueue<ScheduledTask>(removed.ToArray()); //collection overload enumerates collection

            _cts.Cancel(true);
        }

        private async Task HandleCallsbackAsync()
        {
            while (true)
            {
                try
                {
                    if (_taskQueue.IsEmpty)
                        await Task.Delay(-1, _cts.Token);

                    if (!_taskQueue.TryPeek(out var task))
                        continue;

                    var when = DateTimeOffset.FromUnixTimeMilliseconds(task.WhenToRemove) - DateTimeOffset.UtcNow;

                    if (when > TimeSpan.Zero)
                    {
                        await Task.Delay(when, _cts.Token);
                    }

                    if (_taskQueue.TryDequeue(out task))
                        await task.Task(task.Key, task.Object);
                }
                catch (TaskCanceledException)
                {
                    _cts.Dispose();
                    _cts = new CancellationTokenSource();
                }
            }
        }

        private class ScheduledTask
        {
            public object Object { get; set; }
            public long WhenToRemove { get; set; }
            public Func<Guid, object, Task> Task { get; set; }
            public Guid Key { get; set; }
        }
    }
}
