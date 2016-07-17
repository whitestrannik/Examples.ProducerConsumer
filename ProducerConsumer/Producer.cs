using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    internal class Producer : IProducer
    {
        internal Producer(IConsumer consumer) : this(consumer, 1)
        { }

        internal Producer(IConsumer consumer, int threadCount)
        {
            if (threadCount == 0) throw new ArgumentException("Count of threads must be greater then zero.");

            _consumer = consumer;
            _workerCount = threadCount;
            _cancellationTokenSource = new CancellationTokenSource();

            InitWorkers();
        }

        public void Dispose()
        {
            StopWorkers();
        }

   

        #region impl

        int _workerCount;
        IConsumer _consumer;
        CancellationTokenSource _cancellationTokenSource;
        Task[] _workers;

        private void InitWorkers()
        {
            _workers = Enumerable.Range(1, _workerCount)
                .Select(i => { return Task.Run(() => WorkLoop(i, _cancellationTokenSource.Token), _cancellationTokenSource.Token); })
                .ToArray();
        }

        private void StopWorkers()
        {
            _cancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(_workers);
            }
            catch (AggregateException ex)
            {
                foreach (Exception innEx in ex.Flatten().InnerExceptions)
                    if (!(innEx is TaskCanceledException)) throw;
            }
        }

        private void WorkLoop(int index, CancellationToken token)
        {
            Console.WriteLine($"Producer {index} is started");

            var random = new Random();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"Producer {index} is stopped");
                    return;
                }

                Console.WriteLine("\tAction is added");
                _consumer.AddAction(() =>
                {
                    Console.WriteLine("\tAction is executed");
                    Thread.Sleep(new Random().Next(1000));
                });

                Thread.Sleep(random.Next(1000));
            }
        }

        #endregion
    }
}
