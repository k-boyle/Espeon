using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonScheduler {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);
        
        private readonly BinaryHeap<IScheduledTask> _tasks;
        private readonly object _lock = new object();

        private CancellationTokenSource _cts;

        public EspeonScheduler() {
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
                    while ((executeIn = next.ExecuteAt - DateTimeOffset.Now) > MaxDelay) {
                        await Task.Delay(MaxDelay, this._cts.Token);
                    }
                    
                    if (executeIn > TimeSpan.Zero) {
                        await Task.Delay(executeIn, this._cts.Token);
                    }
                    
                    try {
                        await next.Callback();
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
        
        public ScheduledTask<T> DoAt<T>(DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            var newTask = new ScheduledTask<T>(executeAt, state, callback);
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
        
        public ScheduledTask<T> DoIn<T>(TimeSpan executeIn, T state, Func<T, Task> callback) {
            var newTask = new ScheduledTask<T>(DateTimeOffset.Now + executeIn, state, callback);
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