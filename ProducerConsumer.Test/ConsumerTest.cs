using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ProducerConsumer.Test
{
    [TestClass]
    public class ConsumerTest
    {
        [TestMethod]
        public void AddAndWaitSingleWorkItemTest()
        {
            int i = 0;
            using (IConsumer consumer = new Consumer())
            {
                Task task = consumer.AddAction(() => { Interlocked.Increment(ref i); });
                task.Wait();
                Assert.IsTrue(i == 1);
                Assert.IsTrue(task.IsCompleted);
            }
        }

        [TestMethod]
        public void AddAndWaitManyWorkItemTest()
        {
            int i = 0;
            using (IConsumer consumer = new Consumer())
            {
                Task task1 = consumer.AddAction(() => { Interlocked.Increment(ref i); });
                Task task2 = consumer.AddAction(() => { Interlocked.Increment(ref i); });
                Task task3 = consumer.AddAction(() => { Interlocked.Increment(ref i); });

                Task.WaitAll(task1, task2, task3);

                Assert.IsTrue(i == 3);
                Assert.IsTrue(task1.IsCompleted);
                Assert.IsTrue(task2.IsCompleted);
                Assert.IsTrue(task3.IsCompleted); 
            }
        }

        [TestMethod]
        public void CancelSingleWorkItemTest()
        {
            int i = 0;
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            using (IConsumer consumer = new Consumer())
            {
                Task task1 = consumer.AddAction(() => { Interlocked.Increment(ref i); autoResetEvent.WaitOne(); });
                Task task2 = consumer.AddAction(() => { Interlocked.Increment(ref i); }, tokenSource.Token);
                tokenSource.Cancel();
                autoResetEvent.Set();

                try
                {
                    task2.Wait();
                }
                catch (AggregateException ex)
                {
                    var cancelExc = ex.InnerException as TaskCanceledException;
                    Assert.IsNotNull(cancelExc);
                }

                Assert.IsTrue(i == 1);
                Assert.IsTrue(task1.IsCompleted);
                Assert.IsTrue(task2.IsCanceled);
            }
        }

        [TestMethod]
        public void CancelManyWorkItemTest()
        {
            int i = 0;
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            using (IConsumer consumer = new Consumer())
            {
                Task task1 = consumer.AddAction(() => { Interlocked.Increment(ref i); autoResetEvent.WaitOne(); });
                Task task2 = consumer.AddAction(() => { Interlocked.Increment(ref i); }, tokenSource.Token);
                Task task3 = consumer.AddAction(() => { Interlocked.Increment(ref i); }, tokenSource.Token);
                Task task4 = consumer.AddAction(() => { Interlocked.Increment(ref i); });
                tokenSource.Cancel();
                autoResetEvent.Set();

                try
                {
                    Task.WaitAll(task1, task2, task3, task4);
                }
                catch (AggregateException ex)
                {
                    foreach(Exception innEx in ex.Flatten().InnerExceptions)
                        Assert.IsTrue(innEx is TaskCanceledException);
                }

                Assert.IsTrue(i == 2);
                Assert.IsTrue(task1.IsCompleted);
                Assert.IsTrue(task2.IsCanceled);
                Assert.IsTrue(task3.IsCanceled);
                Assert.IsTrue(task4.IsCompleted);
            }
        }

        [TestMethod]
        public void ExecuteOrderTest()
        {
            int i = 0;
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            using (IConsumer consumer = new Consumer())
            {
                Task task1 = consumer.AddAction(() => { queue.Enqueue(1); });
                Task task2 = consumer.AddAction(() => { queue.Enqueue(2); });
                Task task3 = consumer.AddAction(() => { queue.Enqueue(3); });
                Task task4 = consumer.AddAction(() => { queue.Enqueue(4); });

                Task.WaitAll(task1, task2, task3, task4);

                Assert.IsTrue(queue.TryDequeue(out i) && i == 1);
                Assert.IsTrue(queue.TryDequeue(out i) && i == 2);
                Assert.IsTrue(queue.TryDequeue(out i) && i == 3);
                Assert.IsTrue(queue.TryDequeue(out i) && i == 4);
            }
        }
    }
}
