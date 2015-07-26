namespace Asynqueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class AsynqueueAwaitable<T> : IAwaitable<T>
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
