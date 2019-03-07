using System;
using System.Collections.Generic;
using System.Threading;

namespace StaThreadSyncronizer
{
    internal interface IFilumReader<T> : IDisposable
    {
        T Peek();
        void ReleaseReader();
    }

    internal interface IFilumWriter<T> : IDisposable
    {
        void AddItem(T data);
    }

    /// <summary>
    /// XスレッドからSTAスレッドへ操作項目を移動するリスト
    /// 操作項目がリストへ移動されてはじめて、リストから取り出す
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BlockingFilum<T> : IFilumReader<T>, IFilumWriter<T>, IDisposable
    {
        private List<T> mFilum = new List<T>();
        // リストに置いてある操作項目を数えるセマフォを作成する
        // セマフォのイニシャライズ (デフォルトバリューは０)
        private Semaphore mSemaphore = new Semaphore(0, int.MaxValue);
        // リーダースレッドを終了している間にトリガーを起動するイベント
        private ManualResetEvent mThreadKiller = new ManualResetEvent(false);

        /// <summary>
        /// ピーク操作または削除操作のブロックを解除する期待ハンドル
        /// </summary>
        private readonly WaitHandle[] mWaitHandles;

        /// <summary>
        /// Blocking Filumコンストラクター
        /// </summary>
        internal BlockingFilum()
        {
            mWaitHandles = new WaitHandle[2] { mSemaphore, mThreadKiller };
        }

        /// <summary>
        /// リストに操作項目を一つしか入れ込みませんので、セマフォも一つずつ増える
        /// </summary>
        /// <param name="data">リストに入れ込む操作項目</param>
        public void AddItem(T data)
        {
            lock (mFilum) mFilum.Add(data);
            mSemaphore.Release();
        }

        /// <summary>
        /// 何かの操作項目がリストに表示するまで期待して、その操作項目を起動する
        /// <remark>
        /// リストに操作項目を入れ込むまで期待する
        /// </remark>
        /// </summary>
        /// <returns>起動させるSendOrPostCallbackItemを返す</returns>
        public T Peek()
        {
            WaitHandle.WaitAny(mWaitHandles);
            lock (mFilum)
            {
                if (mFilum.Count > 0)
                {
                    T mCallBack = mFilum[0];
                    mFilum.RemoveAt(0);
                    return mCallBack;
                }
            }
            return default(T);
        }

        /// <summary>
        /// リストから特定の操作項目を削除する
        /// </summary>
        /// <param name="item">削除される操作項目/param>
        public void RemoveItem(T item)
        {
            WaitHandle.WaitAny(mWaitHandles);
            lock (mFilum)
            {
                if (mFilum.Count > 0)
                {
                    mFilum.Remove(item);
                }
            }
        }

        /// <summary>
        /// クラスコンテストが処分されたはじめて、リーダーを停止する
        /// </summary>
        public void ReleaseReader()
        {
            mThreadKiller.Set();
        }

        /// <summary>
        /// クラスを処分すると、セマフォをブロックしてリストを初期化する
        /// </summary>
        void IDisposable.Dispose()
        {
            if (mSemaphore != null)
            {
                mSemaphore.Close();
                mFilum.Clear();
                mSemaphore = null;
            }
        }
    }
}
