using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    public interface IConsumer : IDisposable
    {
        /// <summary>
        /// Adding new action for executing
        /// </summary>
        /// <param name="action">Action for executing</param>
        /// <returns>Task to control action procession</returns>
        Task AddAction(Action action);

        /// <summary>
        /// Adding new action for executing with posibility to cancel
        /// </summary>
        /// <param name="action">Action for executing</param>
        /// <param name="token">CancellationToken for canceling action</param>
        /// <returns></returns>
        Task AddAction(Action action, CancellationToken token);
    }
}
