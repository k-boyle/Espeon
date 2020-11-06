using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    // todo common
    public class EspeonScheduler : IDisposable {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);
        private static readonly Action<EspeonScheduler> DoAtCleanup = @this => @this._tasks.TryRemoveRoot(out _);
        private static readonly Action<EspeonScheduler> DoNowCleanup = _ => { };

        public event Action<Exception> OnError;
        
        private readonly ILogger<EspeonScheduler> _logger;
        private readonly LockedBinaryHeap<IScheduledTask> _tasks;
        private readonly ConcurrentQueue<IScheduledTask> _doNowTasks;

        private CancellationTokenSource _cts;
        private volatile bool _disposed;

        public EspeonScheduler(ILogger<EspeonScheduler> logger) {
            this._logger = logger;
            this._tasks = LockedBinaryHeap<IScheduledTask>.CreateMinHeap();
            this._doNowTasks = new ConcurrentQueue<IScheduledTask>();
            this._cts = new CancellationTokenSource();
            _ = TaskLoopAsync();
        }

        private async Task TaskLoopAsync() {
            while (!this._disposed) {
                try {
                    await DoDoNowsAsync();
                    await PauseLoopAsync();
                    var nextTask = this._tasks.Root;
                    await DelayUntilTaskCanExecuteAsync(nextTask);
                    await ExecuteTaskAsync(nextTask, DoAtCleanup);
                } catch (TaskCanceledException) {
                    this._cts.Dispose();
                    this._cts = new CancellationTokenSource();
                }
            }
        }

        private async Task DoDoNowsAsync() {
            while (this._doNowTasks.TryDequeue(out var task)) {
                await ExecuteTaskAsync(task, DoNowCleanup);
            }
        }

        private async Task PauseLoopAsync() {
            if (this._tasks.IsEmpty) {
                await Task.Delay(-1, this._cts.Token);
            }
        }

        private async Task DelayUntilTaskCanExecuteAsync(IScheduledTask task) {
            TimeSpan executeIn;
            this._logger.LogDebug("Waiting for {task} in {duration}", task.Name, task.ExecuteAt);
            while ((executeIn = task.ExecuteAt - DateTimeOffset.Now) > MaxDelay) {
                await Task.Delay(MaxDelay, this._cts.Token);
            }

            if (executeIn > TimeSpan.Zero) {
                await Task.Delay(executeIn, this._cts.Token);
            }
        }

        private async Task ExecuteTaskAsync(IScheduledTask task, Action<EspeonScheduler> cleanup) {
            try {
                if (!task.IsCancelled) {
                    this._logger.LogDebug("Executing {task}", task.Name);
                    await task.Callback();
                } else {
                    this._logger.LogDebug("{task} was cancelled", task.Name);
                }
            } catch (Exception eventException) {
                try {
                    OnError?.Invoke(eventException);
                } catch (Exception onErrorException) {
                    this._logger.LogError("Exception thrown by OnError handler", onErrorException);
                }
            } finally {
                task.Completed();
                cleanup(this);
            }
        }
        
        public ScheduledTask<T> DoNow<T>(T state, Func<T, Task> callback) {
            return DoNow(null, state, callback);
        }
        
        public ScheduledTask<T> DoNow<T>(string name, T state, Func<T, Task> callback) {
            CheckNotDisposed();
            
            var newTask = new ScheduledTask<T>(name, DateTimeOffset.Now, state, callback);
            this._logger.LogDebug("Queueing up {task}", newTask.Name);
            this._doNowTasks.Enqueue(newTask);
            this._cts.Cancel(true);
            return newTask;
        }

        public ScheduledTask<T> DoIn<T>(TimeSpan executeIn, T state, Func<T, Task> callback) {
            return DoIn(null, executeIn, state, callback);
        }

        public ScheduledTask<T> DoIn<T>(string name, TimeSpan executeIn, T state, Func<T, Task> callback) {
            if (executeIn == TimeSpan.MaxValue) {
                throw new InvalidOperationException($"Can only execute up to {DateTimeOffset.MaxValue - DateTimeOffset.Now}");
            }
            
            return DoAt(name, DateTimeOffset.Now + executeIn, state, callback);
        }

        public ScheduledTask<T> DoAt<T>(DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            return DoAt(null, executeAt, state, callback);
        }

        public ScheduledTask<T> DoAt<T>(string name, DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            CheckNotDisposed();
            
            var newTask = new ScheduledTask<T>(name, executeAt, state, callback);
            this._logger.LogDebug("Queueing up {task}", newTask.Name);
            var root = this._tasks.Root;
            this._tasks.Insert(newTask);
            
            if (!ReferenceEquals(root, this._tasks.Root)) {
                this._cts.Cancel(true);
            }

            return newTask;
        }
        
        private void CheckNotDisposed() {
            if (this._disposed) {
                throw new ObjectDisposedException(nameof(EspeonScheduler));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing) {
            if (this._disposed) {
                return;
            }

            if (!disposing) {
                return;
            }

            this._disposed = true;
            this._cts.Cancel(true);
            this._cts?.Dispose();
            this._cts = null;
        }
    }
}