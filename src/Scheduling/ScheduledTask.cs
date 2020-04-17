using System;
using System.Threading.Tasks;

namespace Espeon {
    public readonly struct ScheduledTask<T> : IScheduledTask {
        public DateTimeOffset ExecuteAt { get; }
        public T State { get; }
        public Func<Task> Callback { get; }

        public ScheduledTask(DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            ExecuteAt = executeAt;
            State = state;
            Callback = () => callback(state);
        }

        public int CompareTo(IScheduledTask other) {
            return ExecuteAt.CompareTo(other.ExecuteAt);
        }
    }
}