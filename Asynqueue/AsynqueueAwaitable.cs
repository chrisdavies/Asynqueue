namespace Asynqueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    
    /// <summary>
    /// AsynqueueAwaitable is the meat of the Asynqueue system. It's a bit complicated, but basically,
    /// this is an awaitable object, with each call to await returning an item off of the queue. This 
    /// serves as a much more efficient way to communicate between threads than traditional signals.
    /// The only portion of this which really needs to be synchronized is the queue access. Only
    /// one thread should ever await this at any given time.
    /// </summary>
    /// <typeparam name="T">The type of message being queued.</typeparam>
    internal class AsynqueueAwaitable<T> : IAwaitable<T>
    {
        private Action continuation;
        private volatile int count = 0;
        private volatile int runCount = 0;
        private Queue<T> q = new Queue<T>();

        public bool IsCompleted
        {
            get
            {
                return count > 0;
            }
        }

        public void Enqueue(T message)
        {
            lock (q)
            {
                Interlocked.Increment(ref count);
                q.Enqueue(message);
            }

            Execute();
        }

        public IAwaitable<T> GetAwaiter()
        {
            return this;
        }

        public T GetResult()
        {
            lock (q)
            {
                Interlocked.Decrement(ref count);
                return q.Dequeue();
            }
        }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;

            Execute();
        }

        private void Execute()
        {
            var call = continuation;

            if (call != null && count > 0 && Interlocked.CompareExchange(ref runCount, 1, 0) == 0)
            {
                call();
                runCount = 0;
                Execute();
            }
        }
    }
}
