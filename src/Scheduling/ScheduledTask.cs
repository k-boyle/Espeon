using System;
using System.Threading.Tasks;

namespace Espeon {
    public readonly struct ScheduledTask<T> : IScheduledTask {
        private static int _taskCounter;
        
        public DateTimeOffset ExecuteAt { get; }
        public T State { get; }
        public Func<Task> Callback { get; }
        public string Name { get; }

        public ScheduledTask(DateTimeOffset executeAt, T state, Func<T, Task> callback)
            : this(null, executeAt, state, callback) { }
        
        public ScheduledTask(string name, DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            Name = name ?? string.Concat("Task: ", _taskCounter++);
            ExecuteAt = executeAt;
            State = state;
            Callback = () => callback(state);
        }

        public int CompareTo(IScheduledTask other) {
            return ExecuteAt.CompareTo(other.ExecuteAt);
        }
    }
}