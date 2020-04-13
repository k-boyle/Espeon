using System;
using System.Threading.Tasks;

namespace Espeon {
    public readonly struct ScheduledTask<T> : IScheduledTask {
        public DateTimeOffset ExecuteAt { get; }
        public T State { get; }
        public Func<Task> Callback { get; }

        private readonly Guid _guid;

        public ScheduledTask(DateTimeOffset executeAt, T state, Func<T, Task> callback) {
            ExecuteAt = executeAt;
            State = state;
            Callback = () => callback(state);
            this._guid = Guid.NewGuid();
        }

        public override bool Equals(object obj) {
            if (!(obj is ScheduledTask<T> other)) {
                return false;
            }

            return this._guid == other._guid;
        }

        public override int GetHashCode() {
            return this._guid.GetHashCode();
        }
    }
}