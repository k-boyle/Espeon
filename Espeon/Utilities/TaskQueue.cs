using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Casino.Common
{
    /// <summary>
    /// A simple task scheduler.
    /// </summary>
    public class TaskQueue : IDisposable
    {
        private ConcurrentQueue<ScheduledTask> _taskQueue;
        private CancellationTokenSource _cts;

        private readonly object _queueLock;

        private TaskQueue()
        {
            _taskQueue = new ConcurrentQueue<ScheduledTask>();
            _cts = new CancellationTokenSource();

            _queueLock = new object();
        }

        /// <summary>
        /// Event that fires whenever there is an exception from a scheduled task.
        /// </summary>
        public event Func<Exception, Task> Error;

        /// <summary>
        /// Create a new instance of the <see cref="TaskQueue"/>.
        /// </summary>
        /// <returns>A <see cref="TaskQueue"/>.</returns>
        public static TaskQueue Create()
        {
            var scheduler = new TaskQueue();

            _ = scheduler.HandleCallsbackAsync();

            return scheduler;
        }

        /// <summary>
        /// Schedule a new task.
        /// </summary>
        /// <param name="obj">An object this task depends upon.</param>
        /// <param name="whenToRemove">The UNIX time in milliseconds of when this task needs to be executed.</param>
        /// <param name="task">The task to be executed.</param>
        /// <returns>A <see cref="Guid"/> that is unique for this task.</returns>
        public Guid ScheduleTask(object obj, long whenToRemove, Func<Guid, object, Task> task)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TaskQueue));

            lock (_queueLock)
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
        }

        /// <summary>
        /// Cancels a currently queued task.
        /// </summary>
        /// <param name="key">The unique <see cref="Guid"/> for the task you want to cancel.</param>
        public void CancelTask(Guid key)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TaskQueue));

            lock (_queueLock)
            {
                var removed = _taskQueue.Where(x => x.Key != key).OrderBy(x => x.WhenToRemove);
                _taskQueue = new ConcurrentQueue<ScheduledTask>(removed.ToArray()); //collection overload enumerates collection

                _cts.Cancel(true);
            }
        }

        private async Task HandleCallsbackAsync()
        {
            while (true)
            {
                if (_disposed)
                    break;

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
                catch (Exception e)
                {
                    await (Error is null ? Task.CompletedTask : Error(e));
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

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts.Cancel(true);
                    _cts.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the <see cref="TaskQueue"/> and frees up any managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
