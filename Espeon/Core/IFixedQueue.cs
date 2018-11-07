namespace Espeon.Core
{
    public interface IFixedQueue<T>
    {
        bool TryEnqueue(T item);
        bool TryDequeue(out T item);
    }
}
