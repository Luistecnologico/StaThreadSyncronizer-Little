using System;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// Contains the delegate we wish to execute on the STA thread
    /// </summary>
    internal class SendOrPostCallbackItem
    {
        internal readonly SendOrPostCallback mMethod;
        internal ManualResetEvent mAsyncWaitHandle = new ManualResetEvent(false);
        internal ManualResetEvent mAsyncQueueWaitHandle = new ManualResetEvent(false);
        internal Exception mException { get; private set; } = null;

        /// <summary>
        /// Delegate we wish to execute
        /// </summary>
        /// <param name="callback">To do method</param>
        /// <param name="state">Parameters of the over method</param>
        /// <param name="type">Delegate running mode: Send or Post</param>
        internal SendOrPostCallbackItem(SendOrPostCallback callback)
        {
            mMethod = callback;
        }

        internal bool ExecutedWithException
        {
            get { return mException != null; }
        }

        /// <summary>
        /// this code must run ont the STA thread
        /// </summary>
        internal void Execute()
        {
            Send();
        }

        /// <summary>
        /// calling thread will block until mAsyncWaitHanel is set
        /// </summary>
        internal void Send()
        {
            try
            {
                //Set Queue Hadnler ON
                mAsyncQueueWaitHandle.Set();
                //call the thread
                mMethod(null);
            }
            catch (Exception e)
            {
                mException = e;
            }
            finally
            {
                mAsyncWaitHandle.Set();
            }
        }

        internal WaitHandle ExecutionCompleteWaitHandle
        {
            get { return mAsyncWaitHandle; }
        }

        internal WaitHandle DequeueCompleteWaitHandle
        {
            get { return mAsyncQueueWaitHandle; }
        }
    }
}
