using System;
using System.Threading.Tasks;

namespace Espeon {
    public interface IScheduledTask : IComparable<IScheduledTask> {
        public DateTimeOffset ExecuteAt { get; }
        public Func<Task> Callback { get; }
    }
}