using System;

namespace Espeon {
    public class BinaryHeap<T> where T : IComparable<T> {
        public T Root => Size > 0 ? this._heap[0] : default;
        public int Size { get; private set; }
        public bool IsEmpty => Size == 0;

        private T[] _heap;

        private readonly int _compareResult;

        private BinaryHeap(int initialHeapSize, int compareResult) {
            if (initialHeapSize < 1) {
                throw new IndexOutOfRangeException("Initial heap size must be >= 1");
            }
            
            this._heap = new T[initialHeapSize];
            this._compareResult = compareResult;
        }
        
        public static BinaryHeap<T> CreateMinHeap(int initialHeapSize = 16) {
            return new BinaryHeap<T>(initialHeapSize, -1);
        }
        
        public static BinaryHeap<T> CreateMaxHeap(int initialHeapSize = 16) {
            return new BinaryHeap<T>(initialHeapSize, 1);
        }

        public void Insert(T node) {
            if (++Size > this._heap.Length) {
                Array.Resize(ref this._heap, this._heap.Length * 2);
            }

            var index = Size - 1;
            this._heap[index] = node;
            
            while (index > 0) {
                var parentIndex = (index - 1) / 2;
                var parent = this._heap[parentIndex];
                
                if (node.CompareTo(parent) != this._compareResult) {
                    break;
                }

                this._heap[index] = parent;
                this._heap[index = parentIndex] = node;
            }
        }
        
        public bool TryRemoveRoot(out T root) {
            root = Root;
            if (Size == 0) {
                return false;
            }
            
            var index = 0;
            var atIndex = this._heap[index] = this._heap[--Size];
            
            for (int leftIndex; (leftIndex = 2 * index + 1) < Size; ) {
                var rightIndex = 2 * index + 2;
                (T Leaf, int Index) toCompare;
                var left = this._heap[leftIndex];
                
                if (rightIndex < Size) {
                    var right = this._heap[rightIndex];
                    toCompare = left.CompareTo(right) == -this._compareResult
                        ? (right, rightIndex)
                        : (left, leftIndex);
                } else {
                    toCompare = (left, leftIndex);
                }
                
                if (atIndex.CompareTo(toCompare.Leaf) == this._compareResult) {
                    break;
                }

                this._heap[index] = toCompare.Leaf;
                this._heap[index = toCompare.Index] = atIndex;
            }

            //do i really care about reducing size?
            if (Size < this._heap.Length / 2) {
                Array.Resize(ref this._heap, this._heap.Length / 2);
            }
            
            return true;
        }
    }
}