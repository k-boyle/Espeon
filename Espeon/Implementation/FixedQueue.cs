using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Espeon.Core;

namespace Espeon.Implementation
{
    public class FixedQueue<T> : ConcurrentQueue<T>, IFixedQueue<T>
    {
        private readonly int _size;

        public FixedQueue(int size)
        {
            _size = size;
        }

        public FixedQueue(int size, ICollection<T> collection) : base(collection)
        {
            if (collection.Count > size)
                throw new ArgumentOutOfRangeException(nameof(collection));

            _size = size;
        }

        public bool TryEnqueue(T item)
        {
            if (Count == _size)
            {
                if (!TryDequeue(out _))
                {
                    return false;
                }
            }

            Enqueue(item);
            return true;
        }
    }
}
