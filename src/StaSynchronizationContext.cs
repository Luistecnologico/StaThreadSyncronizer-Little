using System;
using System.Security.Permissions;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// It is responsible to marshal code into an STA thread, allowing the caller to execute COM APIs that must be on an STA thread.
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
    public class StaSynchronizationContext : SynchronizationContext, IDisposable
    {
        private BlockingQueue<SendOrPostCallbackItem> mQueue;
        private StaThread mStaThread;

        public StaSynchronizationContext()
           : base()
        {
            mQueue = new BlockingQueue<SendOrPostCallbackItem>();
            mStaThread = new StaThread(mQueue);
            mStaThread.Start();
        }

        /// <summary>
        /// A lambda Action and the maximum waiting time in the queue are passed to it by parameter.
        /// </summary>
        /// <param name="action">Action passed as a lambda expression</param>
        /// <param name="milisecondsTimeout">Blocks the current thread until the current WaitHandle receives a signal, using a 32-bit signed integer to specify the time interval in milliseconds.</param>
        public void Send(Action action, int milisecondsTimeout = -1)
        {
            // Dispatches an asynchronous message to context
            SendOrPostCallback d = new SendOrPostCallback(_ => action());

            // create an item for execution
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d);
            // queue the item
            mQueue.Enqueue(item);
            // wait for the item execution to end
            if (item.DequeueCompleteWaitHandle.WaitOne(milisecondsTimeout))
                item.ExecutionCompleteWaitHandle.WaitOne();
            else
                mQueue.Dequeue(item);

            // if there was an exception, throw it on the caller thread, not the
            // sta thread.
            if (item.ExecutedWithException)
                throw item.mException;
        }

        public void Dispose()
        {
            mStaThread.Stop();
        }
    }
}