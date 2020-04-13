using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    public class EspeonScheduler {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);
        
        private readonly SortedSet<IScheduledTask> _tasks;
        private readonly object _lock = new object();

        private CancellationTokenSource _cts;

        public EspeonScheduler() {
            this._tasks = new SortedSet<IScheduledTask>(ScheduledTaskComparer.Instance);
            this._cts = new CancellationTokenSource();
            _ = TaskLoopAsync();
        }

        public event Action<Exception> OnError;

        private async Task TaskLoopAsync() {
            while (true) {
                try {
                    IScheduledTask next;
                    lock (this._lock) {
                        next = this._tasks.Min;
                    }
                    
                    if (next is null) {
                        await Task.Delay(-1, this._cts.Token);
                    }
                    
                    TimeSpan executeIn;
                    Debug.Assert(next != null);
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
                        this._tasks.Remove(next);
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
                if (newTask.ExecuteAt < this._tasks.Min?.ExecuteAt) {
                    this._tasks.Add(newTask);
                    this._cts.Cancel(true);
                } else {
                    this._tasks.Add(newTask);
                }

                return newTask;
            }
        }
        
        public ScheduledTask<T> DoIn<T>(TimeSpan executeIn, T state, Func<T, Task> callback) {
            var newTask = new ScheduledTask<T>(DateTimeOffset.Now + executeIn, state, callback);
            lock (this._lock) {
                if (newTask.ExecuteAt < this._tasks.Min?.ExecuteAt) {
                    this._tasks.Add(newTask);
                    this._cts.Cancel(true);
                } else {
                    this._tasks.Add(newTask);
                }

                return newTask;
            }
        }
    }
}