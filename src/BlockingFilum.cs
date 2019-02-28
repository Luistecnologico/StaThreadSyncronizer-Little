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
    /// Queue to queue up work items from thread X to my STA thread
    /// and dequeue items only when there are items in the queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BlockingFilum<T> : IFilumReader<T>, IFilumWriter<T>, IDisposable
    {
        private List<T> mFilum = new List<T>();
        // Create a semaphore that counts the items in the Filum as resources.
        // Initialize the semaphore (default value 0)
        private Semaphore mSemaphore = new Semaphore(0, int.MaxValue);
        // A event that gets triggered when the reader thread is exiting
        private ManualResetEvent mThreadKiller = new ManualResetEvent(false);

        /// <summary>
        /// Wait handles that are used to unblock a Peek or Remove operations.
        /// </summary>
        private readonly WaitHandle[] mWaitHandles;

        /// <summary>
        /// Blocking Filum Constructor
        /// </summary>
        internal BlockingFilum()
        {
            mWaitHandles = new WaitHandle[2] { mSemaphore, mThreadKiller };
        }

        /// <summary>
        /// Because it's just puted an item into the filum, add and available resource to the semaphore
        /// </summary>
        /// <param name="data">Item that will be stored into the Filum</param>
        public void AddItem(T data)
        {
            lock (mFilum) mFilum.Add(data);
            mSemaphore.Release();
        }

        /// <summary>
        /// Wait until something pops into the filum and return the Next Item to launch.
        /// <remark>
        /// Wait until there is an item in the filum.
        /// </remark>
        /// </summary>
        /// <returns>return Next SendOrPostCallbackItem</returns>
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
        /// Remove from the Filum a specific item
        /// </summary>
        /// <param name="item">Item that will be removed</param>
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
        /// Stop the Reader (and, so, its threaders) when context class is disposed.
        /// </summary>
        public void ReleaseReader()
        {
            mThreadKiller.Set();
        }

        /// <summary>
        /// When this class is disposed, close Semaphore (to not accept new requests) and clear the Filum.
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
