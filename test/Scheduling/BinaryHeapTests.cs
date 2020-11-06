using NUnit.Framework;
using System;

namespace Espeon.Test {
    public class BinaryHeapTests {
        [Test]
        public void TestInvalidMinHeapSizeThrows() {
            Assert.Throws<IndexOutOfRangeException>(() => BinaryHeap<int>.CreateMinHeap(-1));
        }

        [Test]
        public void TestMinHeapEmpty() {
            var heap = BinaryHeap<int>.CreateMinHeap();
            Assert.True(heap.IsEmpty);
        }

        [Test]
        public void TestMinHeapInsert() {
            var heap = BinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMinHeapResize() {
            var heap = BinaryHeap<int>.CreateMinHeap(2);
            heap.Insert(10);
            heap.Insert(20);
            heap.Insert(30);
            Assert.True(heap.Size > 2);
        }

        [Test]
        public void TestMinHeapRootChanges() {
            var heap = BinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            heap.Insert(4);
            Assert.AreEqual(4, heap.Root);
        }

        [Test]
        public void TestMinHeapRemoveFalseWhenEmpty() {
            var heap = BinaryHeap<int>.CreateMinHeap();
            Assert.False(heap.TryRemoveRoot(out var r));
        }

        [Test]
        public void TestMinHeapRemovesRoot() {
            var heap = BinaryHeap<int>.CreateMinHeap();
            heap.Insert(10);
            heap.Insert(20);
            var removed = heap.TryRemoveRoot(out var root);
            Assert.True(removed);
            Assert.AreEqual(10, root);
            Assert.AreEqual(20, heap.Root);
        }

        [Test]
        public void TestMinHeapOrderedRemoval() {
            var heap = BinaryHeap<int>.CreateMinHeap();
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
            Assert.Throws<IndexOutOfRangeException>(() => BinaryHeap<int>.CreateMaxHeap(-1));
        }

        [Test]
        public void TestMaxHeapEmpty() {
            var heap = BinaryHeap<int>.CreateMaxHeap();
            Assert.True(heap.IsEmpty);
        }

        [Test]
        public void TestMaxHeapInsert() {
            var heap = BinaryHeap<int>.CreateMaxHeap();
            heap.Insert(10);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMaxHeapResize() {
            var heap = BinaryHeap<int>.CreateMaxHeap(2);
            heap.Insert(10);
            heap.Insert(20);
            heap.Insert(30);
            Assert.True(heap.Size > 2);
        }

        [Test]
        public void TestMaxHeapRemoveFalseWhenEmpty() {
            var heap = BinaryHeap<int>.CreateMaxHeap();
            Assert.False(heap.TryRemoveRoot(out var r));
        }

        [Test]
        public void TestMaxHeapRemovesRoot() {
            var heap = BinaryHeap<int>.CreateMaxHeap();
            heap.Insert(10);
            heap.Insert(20);
            var removed = heap.TryRemoveRoot(out var root);
            Assert.True(removed);
            Assert.AreEqual(20, root);
            Assert.AreEqual(10, heap.Root);
        }

        [Test]
        public void TestMaxHeapOrderedRemoval() {
            var heap = BinaryHeap<int>.CreateMaxHeap();
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