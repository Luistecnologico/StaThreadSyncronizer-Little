using System.Threading;

namespace StaThreadSyncronizer
{
    internal class StaThread
    {
        private Thread mStaThread;
        private IQueueReader<SendOrPostCallbackItem> mQueueConsumer;
        /// <summary>
        /// Thread where the code is running
        /// </summary>
        internal int ManagedThreadId { get; private set; }
        private ManualResetEvent mStopEvent = new ManualResetEvent(false);

        /// <summary>
        /// This class takes an interface of type IQueueReader, this is really our blocking queue.
        /// The thread is being setup as an STA thread.
        /// </summary>
        /// <param name="reader"></param>
        internal StaThread(IQueueReader<SendOrPostCallbackItem> reader)
        {
            mQueueConsumer = reader;
            mStaThread = new Thread(Run);
            mStaThread.Name = "STA Worker Thread";
            mStaThread.SetApartmentState(ApartmentState.STA);
        }

        internal void Start()
        {
            mStaThread.Start();
        }

        /// <summary>
        /// Executing any work items on the Run method means executing them on the STA thread.
        /// </summary>
        private void Run()
        {
            ManagedThreadId = Thread.CurrentThread.ManagedThreadId;
            while (true)
            {
                bool stop = mStopEvent.WaitOne(0);
                if (stop)
                {
                    break;
                }

                SendOrPostCallbackItem workItem = mQueueConsumer.Dequeue();
                if (workItem != null)
                {
                    workItem.Execute();
                }
                    
            }
        }

        internal void Stop()
        {
            mStopEvent.Set();
            mQueueConsumer.ReleaseReader();
            mStaThread.Join();
            mQueueConsumer.Dispose();
        }
    }
}
