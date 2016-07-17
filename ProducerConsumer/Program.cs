using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            int consumerThreadCount = 1;
            bool isInterruptImmidiately = true; // Not execute remaining actions when stopping
            int producerThreadCount = 3;

            Console.WriteLine("For interruption produce-consume process press Enter");
            Console.WriteLine("Press Enter to start...");
            Console.ReadLine();

            try
            {
                using (IConsumer consumer = Factory.CreateConsumer(consumerThreadCount, isInterruptImmidiately))
                using (IProducer producer = Factory.CreateProducer(consumer, producerThreadCount))
                    Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("Sorry, something went wrong :(");
            Console.WriteLine(e);
        }
    }
}