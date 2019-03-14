using System;
using System.Threading;

namespace StaThreadSyncronizer
{
    /// <summary>
    /// STAスレッドで実行するるデリゲートを含む
    /// </summary>
    internal class SendOrPostCallbackItem : IDisposable
    {
        private readonly SendOrPostCallback mMethod;
        internal ManualResetEvent mExecutionCompleteWaitHandle = new ManualResetEvent(false);
        internal ManualResetEvent mPeekCompleteWaitHandle = new ManualResetEvent(false);
        internal Exception MException { get; private set; } = null;
        /// <summary>
        /// コードで発生されたエラーを返す
        /// </summary>
        internal bool MExecutedWithException => MException != null;

        /// <summary>
        /// 実行したいデリゲート
        /// </summary>
        /// <param name="callback">実行する処理</param>
        internal SendOrPostCallbackItem(SendOrPostCallback callback)
        {
            mMethod = callback;
        }

        /// <summary>
        /// STAスレッドで起動されるコード
        /// mAsyncWaitHanelが呼び出されるまでピック操作がブロックしている
        /// </summary>
        internal void Execute()
        {
            try
            {
                // リストハンドルをオンにする
                mPeekCompleteWaitHandle.Set();
                // スレッドを起動する
                mMethod(null);
            }
            catch (Exception e)
            {
                MException = e;
            }
            finally
            {
                mExecutionCompleteWaitHandle.Set();
            }
        }

        /// <summary>
        /// ManualResetEventと自身を解放
        /// </summary>
        public void Dispose()
        {
            mExecutionCompleteWaitHandle.Close();
            mPeekCompleteWaitHandle.Close();
            GC.SuppressFinalize(this);
        }
    }
}
