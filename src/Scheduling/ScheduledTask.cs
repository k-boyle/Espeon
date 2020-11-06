using System;
using System.Threading.Tasks;

namespace Espeon {
    public class ScheduledTask<T> : IScheduledTask {
        private static int _taskCounter;
        
        public DateTimeOffset ExecuteAt { get; }
        public T State { get; }
        public Func<Task> Callback { get; }
        public string Name { get; }
        public bool IsCancelled { get; private set; }

        private readonly TaskCompletionSource<bool> _taskCompletionSource;

        public ScheduledTask(DateTimeOffset executeAt, T state, Func<T, Task> callback)
            : this(null, executeAt, state, callback) { }
        
        public ScheduledTask(string name, DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            Name = name ?? string.Concat("Task: ", _taskCounter++.ToString());
            ExecuteAt = executeAt;
            State = state;
            Callback = () => callback(state);
            IsCancelled = false;
            this._taskCompletionSource = new TaskCompletionSource<bool>();
        }

        public void Cancel() {
            IsCancelled = true;
        }
        
        public async Task WaitUntilExecutedAsync() {
            await this._taskCompletionSource.Task;
        }
        
        void IScheduledTask.Completed() {
            this._taskCompletionSource.SetResult(true);
        }
        
        public int CompareTo(IScheduledTask other) {
            return ExecuteAt.CompareTo(other.ExecuteAt);
        }
    }
}