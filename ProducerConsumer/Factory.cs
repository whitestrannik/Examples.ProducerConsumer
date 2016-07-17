using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    public static class Factory
    {
        public static IConsumer CreateConsumer(int threadCount = 1, bool waitForAddedWorkItemCompletition = true)
        {
            return new Consumer(threadCount, waitForAddedWorkItemCompletition);
        }

        public static IProducer CreateProducer(IConsumer consumer, int threadCount = 1)
        {
            return new Producer(consumer, threadCount);
        }
    }
}
