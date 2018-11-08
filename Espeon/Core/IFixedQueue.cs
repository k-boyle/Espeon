using System.Collections.Generic;

namespace Espeon.Core
{
    public interface IFixedQueue<T> : IEnumerable<T>
    {
        bool TryEnqueue(T item);
        bool TryDequeue(out T item);
    }
}
