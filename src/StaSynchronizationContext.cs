using System;
using System.Security.Permissions;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// It is responsible to command code into an STA thread.
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
    public class STASynchronizationContext : IDisposable
    {
        private BlockingFilum<SendOrPostCallbackItem> mFilum;
        private StaThread mSTAThread;

        public STASynchronizationContext()
           : base()
        {
            mFilum = new BlockingFilum<SendOrPostCallbackItem>();
            mSTAThread = new StaThread(mFilum);
            mSTAThread.Start();
        }

        /// <summary>
        /// A lambda Action and the maximum waiting time in the filum are passed to it by parameter.
        /// </summary>
        /// <param name="action">Action passed as a lambda expression</param>
        /// <param name="milisecondsTimeOut">Blocks the current thread until the current WaitHandle (mPeekCompleteWaitHandle) receives a signal, 
        /// using a 32-bit signed integer to specify the time interval in milliseconds.
        /// </param>
        public void Send(Action action, int milisecondsTimeOut = -1)
        {
            // Dispatches an asynchronous message to context
            SendOrPostCallback d = new SendOrPostCallback(_ => action());

            // create an item
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d);

            // Add item to the filum
            mFilum.AddItem(item);

            // Wait for the item add to peek
            if (item.mPeekCompleteWaitHandle.WaitOne(milisecondsTimeOut))
                item.mExecutionCompleteWaitHandle.WaitOne(); // <-- Wait for the item execution to end
            else
                mFilum.RemoveItem(item); // <-- Waiting Time is out

            // throw the exception on the caller thread, not the STA thread.
            if (item.mExecutedWithException)
                throw item.mException;
        }

        public void Dispose()
        {
            mSTAThread.Stop();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~STASynchronizationContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose to other classes
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                mSTAThread.Stop();
                mSTAThread.Dispose();
                mFilum.Dispose();
            }
        }
    }
}