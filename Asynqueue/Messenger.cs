namespace Asynqueue
{
    using System;
    using System.Collections.Generic;

    public class Messenger<T>
    {
        private Queue<T> q;
        private MessengerAwaitable<T> notifier;

        public Messenger()
        {
            q = new Queue<T>();
            notifier = new MessengerAwaitable<T>(this);
        }

        public void Send(T message)
        {
            lock (q)
            {
                q.Enqueue(message);
                notifier.Set();
            }
        }

        public MessengerAwaitable<T> Receive()
        {
            return notifier;
        }

        public T Get()
        {
            lock (q)
            {
                return q.Dequeue();
            }
        }

        public void Sync(Action fn)
        {
            lock (q)
            {
                fn();
            }
        }
    }
}
