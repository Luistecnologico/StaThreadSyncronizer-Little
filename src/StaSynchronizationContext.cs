using System;
using System.Security.Permissions;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// XスレッドからSTAスレッドへ操作項目を移動する
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
    public class STASynchronizationContext : SynchronizationContext, IDisposable
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
        /// ラムダ式と最大期待期間をパラメーターとして渡される
        /// </summary>
        /// <param name="action">ラムダ式として渡される</param>
        /// <param name="milisecondsTimeOut">
        /// waitHandle (mPeekCompleteWaitHandle)がシグナルを受け取るまで現在スレッドをブロックする。
        /// ミリで最大期待期間を設定するように３２ビットの整数値を使用する
        /// </param>
        public void Send(Action action, int milisecondsTimeOut = -1)
        {
            // コンテストに非同期処理を移動する
            SendOrPostCallback d = new SendOrPostCallback(_ => action());

            // 項目を作成する
            SendOrPostCallbackItem item = new SendOrPostCallbackItem(d);

            // リストに項目を入れ込む
            mFilum.AddItem(item);

            // 項目が入れ込まれるから取り出されるまで期待する
            if (item.mPeekCompleteWaitHandle.WaitOne(milisecondsTimeOut))
                item.mExecutionCompleteWaitHandle.WaitOne(); // <-- 操作項目実行時を期待する
            else
                mFilum.RemoveItem(item); // <-- 最大期待期間超える場合

            // 元スレッドで例外処理を実行する, STAスレッドで実行しない.
            if (item.mExecutedWithException)
                throw item.mException;
        }

        public void Dispose()
        {
            mSTAThread.Stop();
        }
    }
}