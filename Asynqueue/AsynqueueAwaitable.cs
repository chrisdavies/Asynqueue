namespace Asynqueue
{
    using System;
    using System.Runtime.CompilerServices;

    internal class AsynqueueAwaitable<T> : IAwaitable<T>
    {
        private Action continuation;
        private Asynqueue<T> getter;
        private int cnt;

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
            getter.Sync(() =>
            {
                ++cnt;
                if (this.continuation != null)
                {
                    continuation();
                }
            });
        }

        public void OnCompleted(Action continuation)
        {
            getter.Sync(() =>
            {
                this.continuation = continuation;
                if (cnt > 0)
                {
                    continuation();
                }
            });
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
