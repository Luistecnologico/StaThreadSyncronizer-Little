using System;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// STAスレッドで実行するるデリゲートを含む
    /// </summary>
    internal class SendOrPostCallbackItem
    {
        private readonly SendOrPostCallback mMethod;
        internal ManualResetEvent mExecutionCompleteWaitHandle = new ManualResetEvent(false);
        internal ManualResetEvent mPeekCompleteWaitHandle = new ManualResetEvent(false);
        internal Exception mException { get; private set; } = null;
        /// <summary>
        /// コードで発生されたエラーを返す
        /// </summary>
        internal bool mExecutedWithException => mException != null;

        /// <summary>
        /// 実行したいデリゲート
        /// </summary>
        /// <param name="callback">実行する処理</param>
        internal SendOrPostCallbackItem(SendOrPostCallback callback)
        {
            mMethod = callback;
        }

        /// <summary>
        /// TSTAスレッドで起動されるコード
        /// mAsyncWaitHanelが呼び出されるまでピック操作がブロックしている
        /// </summary>
        internal void Execute()
        {
            try
            {
                //リストハンドルをオンにする
                mPeekCompleteWaitHandle.Set();
                //スレッドを起動する
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
