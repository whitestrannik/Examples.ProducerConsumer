using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    /// <summary>
    /// Class designed to execute added actions in multiple threads
    /// </summary>
    public class Consumer : IConsumer
    {
        /// <summary>
        /// Constructor method with default params (single internal thread and not execute unexecuted actions before stopping)
        /// </summary>
        public Consumer() : this (1, false)
        { }

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="workerCount">Count of threads which will execute actions</param>
        /// <param name="isInterruptImmediately">Is necessary to execute all added workitem before stopping (true - not necessary)</param>
        public Consumer(int workerCount, bool isInterruptImmediately)
        {
            if (workerCount == 0) throw new ArgumentException("Count of worker must be greater then zero.");

            _workerCount = workerCount;
            _isInterruptImmidiately = isInterruptImmediately;
            _workItemCollection = new BlockingCollection<WorkItem>();
            _workerCancellationTokenSource = new CancellationTokenSource();

            InitWorkers();
        }

        /// <summary>
        /// Adding new action for executing
        /// </summary>
        /// <param name="action">Action for executing</param>
        /// <returns>Task to control action procession</returns>
        public Task AddAction(Action action)
        {
            return AddWorkItem(action, null);
        }

        /// <summary>
        /// Adding new action for executing with posibility to cancel
        /// </summary>
        /// <param name="action">Action for executing</param>
        /// <param name="token">CancellationToken for canceling action</param>
        /// <returns></returns>
        public Task AddAction(Action action, CancellationToken token)
        {
            return AddWorkItem(action, token);
        }

        /// <summary>
        /// Stopped all threads
        /// </summary>
        public void Dispose()
        {
            StopWorkers();
            _workItemCollection.Dispose();
        }


        #region impl

        int _workerCount;
        bool _isInterruptImmidiately;
        BlockingCollection<WorkItem> _workItemCollection;
        CancellationTokenSource _workerCancellationTokenSource;
        Task[] _workers;

        class WorkItem
        {
            public WorkItem(Action action, TaskCompletionSource<object> taskCompletitionSource, CancellationToken? cancellationToken)
            {
                Action = action;
                TaskCompletitionSource = taskCompletitionSource;
                CancellationToken = cancellationToken;
            }

            public readonly Action Action;

            public readonly TaskCompletionSource<object> TaskCompletitionSource;

            public readonly CancellationToken? CancellationToken;
        }


        private void InitWorkers()
        {
            _workers = Enumerable.Range(1, _workerCount)
                .Select(i => 
                {
                    return Task.Run(() => WorkLoop(i, _workerCancellationTokenSource.Token), _workerCancellationTokenSource.Token);
                })
                .ToArray();
        }

        private void StopWorkers()
        {
            _workItemCollection.CompleteAdding();

            if (_isInterruptImmidiately) _workerCancellationTokenSource.Cancel();

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

        private Task AddWorkItem(Action action, CancellationToken? token)
        {
            if (action == null) throw new ArgumentNullException("Action param can not be null");

            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            _workItemCollection.Add(new WorkItem(action, taskCompletionSource, token));
            return taskCompletionSource.Task;
        }

        private void WorkLoop(int index, CancellationToken token)
        {
            Console.WriteLine($"Consumer {index} is started");

            try
            {
                foreach (WorkItem taskData in _workItemCollection.GetConsumingEnumerable(token))
                {
                    if (taskData.CancellationToken.HasValue && taskData.CancellationToken.Value.IsCancellationRequested)
                    {
                        taskData.TaskCompletitionSource.SetCanceled();
                        continue;
                    }

                    try
                    {
                        taskData.Action();
                        taskData.TaskCompletitionSource.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        taskData.TaskCompletitionSource.SetException(ex);
                    }
                }
            }
            catch (OperationCanceledException) { }


            Console.WriteLine($"Consumer {index} is stopped");
        }

        #endregion impl
    }
}
