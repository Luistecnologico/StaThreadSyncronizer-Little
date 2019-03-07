using System.Threading;

namespace StaThreadSyncronizer
{
    internal class StaThread
    {
        private Thread mSTAThread;
        private IFilumReader<SendOrPostCallbackItem> mFilumPunter;
        private ManualResetEvent mStopEvent = new ManualResetEvent(false);

        /// <summary>
        /// IQueueReader型インタフェースを使用する。これは、実際のリストのブロック
        /// スレッドはSTAスレッドとして設定している
        /// </summary>
        /// <param name="reader">リストの項目を読み込めるリーダー</param>
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
        /// Runメソッドでワーク項目を実行するとは、STAスレッドで実行するという意味
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
    }
}
