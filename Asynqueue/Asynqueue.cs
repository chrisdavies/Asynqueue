namespace Asynqueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Asynqueue<T> : IDisposable
    {
        private Queue<T> q;
        private AsynqueueAwaitable<T> notifier;
        private Task processor;

        public Asynqueue(Action<T> actor)
        {
            q = new Queue<T>();
            notifier = new AsynqueueAwaitable<T>(this);

            this.processor = Task.Run(async () =>
            {
                while (true)
                {
                    var request = await Receive();
                    actor(request);
                }
            });
        }

        public void Send(T message)
        {
            lock (q)
            {
                q.Enqueue(message);
                notifier.Set();
            }
        }

        protected IAwaitable<T> Receive()
        {
            return notifier;
        }

        internal T Get()
        {
            lock (q)
            {
                return q.Dequeue();
            }
        }

        internal protected void Sync(Action fn)
        {
            lock (q)
            {
                fn();
            }
        }

        public virtual void Dispose()
        {
            if (this.processor != null)
            {
                this.processor.Dispose();
                this.processor = null;
            }
        }
    }
}
