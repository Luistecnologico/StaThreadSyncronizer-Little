using System;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// Contains the delegate we wish to execute on the STA thread
    /// </summary>
    internal class SendOrPostCallbackItem
    {
        private readonly SendOrPostCallback mMethod;
        internal ManualResetEvent mExecutionCompleteWaitHandle = new ManualResetEvent(false);
        internal ManualResetEvent mPeekCompleteWaitHandle = new ManualResetEvent(false);
        internal Exception mException { get; private set; } = null;
        /// <summary>
        /// Return if there was any Exception inside the code
        /// </summary>
        internal bool mExecutedWithException => mException != null;

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

        /// <summary>
        /// This code must run ont the STA thread.
        /// Calling thread will block until mAsyncWaitHanel is set
        /// </summary>
        internal void Execute()
        {
            try
            {
                //Set Filum Hadnler ON
                mPeekCompleteWaitHandle.Set();
                //call the thread
                mMethod(null);
            }
            catch (Exception e)
            {
                mException = e;
            }
            finally
            {
                mExecutionCompleteWaitHandle.Set();
            }
        }
    }
}
