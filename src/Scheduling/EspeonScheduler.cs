using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon {
    // todo common
    public class EspeonScheduler {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);

        private readonly ILogger<EspeonScheduler> _logger;
        private readonly LockedBinaryHeap<IScheduledTask> _tasks;

        private CancellationTokenSource _cts;

        public EspeonScheduler(ILogger<EspeonScheduler> logger) {
            this._logger = logger;
            this._tasks = LockedBinaryHeap<IScheduledTask>.CreateMinHeap();
            this._cts = new CancellationTokenSource();
            _ = TaskLoopAsync();
        }

        public event Action<Exception> OnError;

        private async Task TaskLoopAsync() {
            while (true) {
                try {
                    await PauseLoopAsync();
                    var nextEvent = this._tasks.Root;
                    await DelayUntilEventCanExecuteAsync(nextEvent);
                    await ExecuteEventAsync(nextEvent);
                } catch (TaskCanceledException) {
                    this._cts.Dispose();
                    this._cts = new CancellationTokenSource();
                }
            }
        }

        private async Task PauseLoopAsync() {
            if (this._tasks.IsEmpty) {
                await Task.Delay(-1, this._cts.Token);
            }
        }

        private async Task DelayUntilEventCanExecuteAsync(IScheduledTask next) {
            TimeSpan executeIn;
            this._logger.LogDebug("Waiting for {task} in {duration}", next.Name, next.ExecuteAt);
            while ((executeIn = next.ExecuteAt - DateTimeOffset.Now) > MaxDelay) {
                await Task.Delay(MaxDelay, this._cts.Token);
            }

            if (executeIn > TimeSpan.Zero) {
                await Task.Delay(executeIn, this._cts.Token);
            }
        }

        private async Task ExecuteEventAsync(IScheduledTask next) {
            try {
                if (!next.IsCancelled) {
                    this._logger.LogDebug("Executing {task}", next.Name);
                    await next.Callback();
                } else {
                    this._logger.LogDebug("{task} was cancelled", next.Name);
                }
            } catch (Exception eventException) {
                try {
                    OnError?.Invoke(eventException);
                } catch (Exception onErrorException) {
                    this._logger.LogError("Exception thrown by OnError handler", onErrorException);
                }
            } finally {
                this._tasks.TryRemoveRoot(out _);
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
            var root = this._tasks.Root;
            this._tasks.Insert(newTask);
            
            if (!ReferenceEquals(root, this._tasks.Root)) {
                this._cts.Cancel(true);
            }

            return newTask;
        }
    }
}