using System.Threading;

namespace StaThreadSyncronizer
{
    internal class StaThread : IDisposable
    {
        private Thread mSTAThread;
        private IFilumReader<SendOrPostCallbackItem> mFilumPunter;
        private ManualResetEvent mStopEvent = new ManualResetEvent(false);

        /// <summary>
        /// This class takes an interface of type IQueueReader, this is really our blocking queue.
        /// The thread is being setup as an STA thread.
        /// </summary>
        /// <param name="reader"></param>
        internal StaThread(IFilumReader<SendOrPostCallbackItem> reader)
        {
            mFilumPunter = reader;
            mSTAThread = new Thread(Run)
            {
                Name = "STA Runner Thread"
            };
            mSTAThread.SetApartmentState(ApartmentState.STA);
        }

        internal void Start()
        {
            mSTAThread.Start();
        }

        /// <summary>
        /// Executing any work items on the Run method means executing them on the STA thread.
        /// </summary>
        private void Run()
        {
            while (true)
            {
                bool stop = mStopEvent.WaitOne(0);
                if (stop)
                {
                    break;
                }

                SendOrPostCallbackItem workItem = mFilumPunter.Peek();
                if (workItem != null)
                {
                    workItem.Execute();
                }   
            }
        }

        internal void Stop()
        {
            mStopEvent.Set();
            mFilumPunter.ReleaseReader();
            mSTAThread.Join();
            mFilumPunter.Dispose();
        }

        public void Dispose()
        {
            mStopEvent.Close();
            GC.SuppressFinalize(this);
        }
    }
}
