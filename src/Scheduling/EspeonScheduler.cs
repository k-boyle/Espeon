using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonScheduler {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);

        private readonly ILogger<EspeonScheduler> _logger;
        private readonly BinaryHeap<IScheduledTask> _tasks;
        private readonly object _lock = new object();

        private CancellationTokenSource _cts;

        public EspeonScheduler(ILogger<EspeonScheduler> logger) {
            this._logger = logger;
            this._tasks = BinaryHeap<IScheduledTask>.CreateMinHeap();
            this._cts = new CancellationTokenSource();
            _ = TaskLoopAsync();
        }

        public event Action<Exception> OnError;

        private async Task TaskLoopAsync() {
            while (true) {
                try {
                    bool wait;
                    lock (this._lock) {
                        wait = this._tasks.IsEmpty;
                    }
                    
                    if (wait) {
                        await Task.Delay(-1, this._cts.Token);
                    }
                    
                    TimeSpan executeIn;
                    var next = this._tasks.Root;
                    this._logger.LogTrace("Waiting for {task} in {duration}", next.Name, next.ExecuteAt);
                    while ((executeIn = next.ExecuteAt - DateTimeOffset.Now) > MaxDelay) {
                        await Task.Delay(MaxDelay, this._cts.Token);
                    }
                    
                    if (executeIn > TimeSpan.Zero) {
                        await Task.Delay(executeIn, this._cts.Token);
                    }
                    
                    try {
                        if (!next.IsCancelled) {
                            this._logger.LogTrace("Executing {task}", next.Name);
                            await next.Callback();
                        } else {
                            this._logger.LogDebug("{task} was cancelled", next.Name);
                        }
                    } catch (Exception ex) {
                        OnError?.Invoke(ex);
                    } finally {
                        lock (this._lock) {
                            this._tasks.TryRemoveRoot(out _);
                        }
                    }
                } catch (TaskCanceledException) {
                    this._cts.Dispose();
                    this._cts = new CancellationTokenSource();
                }
            }
        }

        public ScheduledTask<T> DoIn<T>(TimeSpan executeIn, T state, Func<T, Task> callback) {
            return DoIn(null, executeIn, state, callback);
        }

        public ScheduledTask<T> DoIn<T>(string name, TimeSpan executeIn, T state, Func<T, Task> callback) {
            return DoAt(name, DateTimeOffset.Now + executeIn, state, callback);
        }

        public ScheduledTask<T> DoAt<T>(DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            return DoAt(null, executeAt, state, callback);
        }

        public ScheduledTask<T> DoAt<T>(string name, DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            var newTask = new ScheduledTask<T>(name, executeAt, state, callback);
            this._logger.LogDebug("Queueing up {task}", newTask.Name);
            lock (this._lock) {
                if (this._tasks.Root is null || newTask.ExecuteAt < this._tasks.Root.ExecuteAt) {
                    this._tasks.Insert(newTask);
                    this._cts.Cancel(true);
                } else {
                    this._tasks.Insert(newTask);
                }

                return newTask;
            }
        }
    }
}