namespace Asynqueue
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal class AsynqueueAwaitable<T> : IAwaitable<T>
    {
        private Action continuation;
        private Asynqueue<T> getter;
        private int cnt;
        private int processingCount;

        public bool IsCompleted { get { return cnt > 0; } }

        public AsynqueueAwaitable(Asynqueue<T> getter)
        {
            this.getter = getter;
        }

        public IAwaitable<T> GetAwaiter()
        {
            return this;
        }

        public void Set()
        {
            Action call = null;
            getter.Sync(() =>
            {
                ++cnt;
                call = this.continuation;
            });

            Process(call);
        }

        private void Process(Action call)
        {
            if (call != null && Interlocked.CompareExchange(ref processingCount, 1, 0) == 0)
            {
                Task.Run(() =>
                {
                    this.continuation();
                    Interlocked.Exchange(ref processingCount, 0);
                });
            }
        }

        public void OnCompleted(Action continuation)
        {
            Action call = null;
            getter.Sync(() =>
            {
                this.continuation = continuation;
                if (cnt > 0)
                {
                    call = continuation;
                }
            });

            Process(call);
        }

        public T GetResult()
        {
            T val = default(T);
            getter.Sync(() =>
            {
                --cnt;
                val = getter.Get();
            });

            return val;
        }
    }
}
