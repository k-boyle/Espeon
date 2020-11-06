using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Test {
    public class EspeonSchedulerTests {
        private static readonly ILogger<EspeonScheduler> Logger = new NullLogger<EspeonScheduler>();
        private static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(200);

        private static Task EmptyCallback<T>(T state) {
            return Task.CompletedTask;
        }
        
        [Test]
        public async Task TestDoNowExecutesInstantlyAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var task = scheduler.DoNow(10, EmptyCallback);
            var expecectedResult = task.WaitUntilExecutedAsync();
            var actualResult = await Task.WhenAny(expecectedResult, Task.Delay(Tolerance));
            Assert.AreEqual(expecectedResult, actualResult);
        }
        
        [Test]
        public async Task TestDoAtExecutesOnTimeAsync() {
            var executesIn = TimeSpan.FromSeconds(5);
            
            using var scheduler = new EspeonScheduler(Logger);
            var task = scheduler.DoAt(DateTimeOffset.Now.Add(executesIn), 10, EmptyCallback);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var sw = Stopwatch.StartNew();
            var expected = task.WaitUntilExecutedAsync();
            var result = await Task.WhenAny(expected, timeout);
            sw.Stop();
            Assert.AreEqual(expected, result);
            AssertWithinTolerance(executesIn, sw.Elapsed);
        }
        
        [Test]
        public async Task TestDoInExecutesOnTimeAsync() {
            var executesIn = TimeSpan.FromSeconds(5);
            
            using var scheduler = new EspeonScheduler(Logger);
            var task = scheduler.DoIn(executesIn, 10, EmptyCallback);
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var sw = Stopwatch.StartNew();
            var expected = task.WaitUntilExecutedAsync();
            var result = await Task.WhenAny(expected, timeout);
            sw.Stop();
            Assert.AreEqual(expected, result);
            AssertWithinTolerance(executesIn, sw.Elapsed);
        }
        
        [Test]
        public async Task TestDoNowExecutesBeforeOtherTaskAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var task1 = scheduler.DoIn(TimeSpan.FromSeconds(5), 10, EmptyCallback);
            var task2 = scheduler.DoNow(10, EmptyCallback);
            var expectedResult = task2.WaitUntilExecutedAsync();
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var actualResult = await Task.WhenAny(task1.WaitUntilExecutedAsync(), expectedResult, timeout);
            Assert.AreEqual(expectedResult, actualResult);
        }
        
        [Test]
        public async Task TestEalierDoInExecutesBeforeLaterDoInAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var task1 = scheduler.DoIn(TimeSpan.FromSeconds(5), 10, EmptyCallback);
            var task2 = scheduler.DoIn(TimeSpan.FromSeconds(2), 10, EmptyCallback);
            var expectedResult = task2.WaitUntilExecutedAsync();
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var actualResult = await Task.WhenAny(task1.WaitUntilExecutedAsync(), expectedResult, timeout);
            Assert.AreEqual(expectedResult, actualResult);
        }
        
        [Test]
        public async Task TestAllTasksGetExecutedAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var tasks = Enumerable.Range(0, 10)
                .Select(i => scheduler.DoIn(TimeSpan.FromSeconds(i), 10, EmptyCallback))
                .Select(task => task.WaitUntilExecutedAsync());
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var expectedResult = Task.WhenAll(tasks);
            var actualResult = await Task.WhenAny(expectedResult, timeout);
            Assert.AreEqual(actualResult, expectedResult);
        }
        
        [Test]
        public async Task TestUnorderedTasksExecuteInCorrectOrderAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var random = new Random();
            var executedList = new List<int>();
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => {
                    var executeIn = random.Next(10);
                    return scheduler.DoIn(TimeSpan.FromSeconds(executeIn), executeIn, num => {
                        executedList.Add(num);
                        return Task.CompletedTask;
                    });
                })
                .Select(task => task.WaitUntilExecutedAsync());
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var expectedResult = Task.WhenAll(tasks);
            var actualResult = await Task.WhenAny(expectedResult, timeout);
            Assert.AreEqual(actualResult, expectedResult);
            var rolling = -1;
            Assert.IsTrue(executedList.TrueForAll(i => rolling <= (rolling = i)));
        }
        
        [Test]
        public async Task TestCallbackGetsCalledAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var executed = false;
            var task = scheduler.DoIn(TimeSpan.FromSeconds(5), 10, _ => {
                executed = true;
                return Task.CompletedTask;
            });
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var expected = task.WaitUntilExecutedAsync();
            var result = await Task.WhenAny(expected, timeout);
            Assert.AreEqual(expected, result);
            Assert.IsTrue(executed);
        }
        
        [Test]
        public async Task TestOnErrorExecutesWhenCallbackThrowsAsync() {
            const string exceptionMessage = "Espeon";
            
            using var scheduler = new EspeonScheduler(Logger);
            var tcs = new TaskCompletionSource<Exception>();
            scheduler.OnError += ex => tcs.SetResult(ex);
            scheduler.DoIn(TimeSpan.FromSeconds(5), 10, _ => throw new InvalidOperationException(exceptionMessage));
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var expectedResult = tcs.Task;
            var actualResult = await Task.WhenAny(expectedResult, timeout);
            Assert.AreEqual(expectedResult, actualResult);
            var res = await expectedResult;
            Assert.IsTrue(res is InvalidOperationException { Message: exceptionMessage });
        }
        
        [Test]
        public async Task TestOnErrorHandlesExceptionsAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var tcs = new TaskCompletionSource<Exception>();
            scheduler.OnError += ex => {
                tcs.SetResult(ex);
                throw new InvalidOperationException();
            };
            scheduler.DoIn(TimeSpan.FromSeconds(5), 10, _ => throw new InvalidOperationException());
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            var expectedResult = tcs.Task;
            var actualResult = await Task.WhenAny(expectedResult, timeout);
            Assert.AreEqual(expectedResult, actualResult);
        }
        
        [Test]
        public async Task TestCancelledTaskDoesntExecuteAsync() {
            using var scheduler = new EspeonScheduler(Logger);
            var executed = false;
            var task = scheduler.DoIn(TimeSpan.FromSeconds(5), 10, _ => {
                executed = true;
                return Task.CompletedTask;
            });
            var timeout = Task.Delay(TimeSpan.FromSeconds(20));
            task.Cancel();
            var actual = task.WaitUntilExecutedAsync();
            var result = await Task.WhenAny(actual, timeout);
            Assert.AreEqual(actual, result);
            Assert.IsFalse(executed);
        }
        
        [Test]
        public void TestSchedulerHandlesLargeTimeSpans() {
            using var scheduler = new EspeonScheduler(Logger);
            scheduler.DoIn(TimeSpan.FromSeconds(int.MaxValue), 10, EmptyCallback);
        }
        
        [Test]
        public void TestSchedulerThrowsOnMaxTimeSpan() {
            using var scheduler = new EspeonScheduler(Logger);
            Assert.Throws<InvalidOperationException>(() => scheduler.DoIn(TimeSpan.MaxValue, 10, EmptyCallback));
        }
        
        [Test]
        public void TestThrowsWhenDisposed() {
            var scheduler = new EspeonScheduler(Logger);
            scheduler.Dispose();
            Assert.Throws<ObjectDisposedException>(() => scheduler.DoNow(10, EmptyCallback));
        }

        private static void AssertWithinTolerance(TimeSpan expectedTime, TimeSpan actualTime) {
            var expectedMillis = expectedTime.TotalMilliseconds;
            var actualMillis = actualTime.TotalMilliseconds;
            var diff = Math.Abs(expectedMillis - actualMillis);
            Assert.True(diff < Tolerance.TotalMilliseconds);
        }
    }
}