namespace Asynqueue
{
    using System;
    using System.Runtime.CompilerServices;

    public class MessengerAwaitable<T> : INotifyCompletion
    {
        private Action continuation;
        private Messenger<T> getter;
        private int cnt;

        public bool IsCompleted { get { return cnt > 0; } }

        public MessengerAwaitable(Messenger<T> getter)
        {
            this.getter = getter;
        }

        public MessengerAwaitable<T> GetAwaiter()
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
