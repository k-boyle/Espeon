using NUnit.Framework;
using System;

namespace Espeon.Test {
    public class LockedLockedBinaryHeapTests {
        [Test]
        public void TestInvalidMinHeapSizeThrows() {
            Assert.Throws<IndexOutOfRangeException>(() => LockedBinaryHeap<int>.CreateMinHeap(-1));
        }

        [Test]
        public void TestMinHeapEmpty() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            Assert.True(heap.IsEmpty);
        }

        [Test]
        public void TestMinHeapInsert() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMinHeapResize() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap(2);
            heap.Insert(10);
            heap.Insert(20);
            heap.Insert(30);
            Assert.True(heap.Size > 2);
        }

        [Test]
        public void TestMinHeapRootChanges() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            heap.Insert(4);
            Assert.AreEqual(4, heap.Root);
        }

        [Test]
        public void TestMinHeapRemoveFalseWhenEmpty() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            Assert.False(heap.TryRemoveRoot(out var r));
        }

        [Test]
        public void TestMinHeapRemovesRoot() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            heap.Insert(20);
            var removed = heap.TryRemoveRoot(out var root);
            Assert.True(removed);
            Assert.AreEqual(10, root);
            Assert.AreEqual(20, heap.Root);
        }

        [Test]
        public void TestMinHeapOrderedRemoval() {
            var heap = LockedBinaryHeap<int>.CreateMinHeap();
            var random = new Random(0);
            for (int i = 0; i < 1000; i++) {
                heap.Insert(random.Next());
            }

            int last = -1;
            while (heap.TryRemoveRoot(out var root)) {
                Assert.GreaterOrEqual(root, last);
                last = root;
            }
        }

        [Test]
        public void TestInvalidMaxHeapSizeThrows() {
            Assert.Throws<IndexOutOfRangeException>(() => LockedBinaryHeap<int>.CreateMaxHeap(-1));
        }

        [Test]
        public void TestMaxHeapEmpty() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap();
            Assert.True(heap.IsEmpty);
        }

        [Test]
        public void TestMaxHeapInsert() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap();
            heap.Insert(10);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMaxHeapResize() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap(2);
            heap.Insert(10);
            heap.Insert(20);
            heap.Insert(30);
            Assert.True(heap.Size > 2);
        }

        [Test]
        public void TestMaxHeapRemoveFalseWhenEmpty() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap();
            Assert.False(heap.TryRemoveRoot(out var r));
        }

        [Test]
        public void TestMaxHeapRemovesRoot() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap();
            heap.Insert(10);
            heap.Insert(20);
            var removed = heap.TryRemoveRoot(out var root);
            Assert.True(removed);
            Assert.AreEqual(20, root);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMaxHeapOrderedRemoval() {
            var heap = LockedBinaryHeap<int>.CreateMaxHeap();
            var random = new Random(0);
            for (int i = 0; i < 1000; i++) {
                heap.Insert(random.Next());
            }

            int last = int.MaxValue;
            while (heap.TryRemoveRoot(out var root)) {
                Assert.LessOrEqual(root, last);
                last = root;
            }
        }
    }
}