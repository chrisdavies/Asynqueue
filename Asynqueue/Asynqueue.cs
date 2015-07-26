namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    public class Asynqueue<T> : IDisposable
    {
        private Task processor;
        private AsynqueueAwaitable<T> awaitableQ = new AsynqueueAwaitable<T>();

        public Asynqueue(Action<T> handler)
        {
            this.processor = Task.Run(async () =>
            {
                while (true)
                {
                    var request = await awaitableQ;
                    handler(request);
                }
            });
        }

        public void Send(T message)
        {
            awaitableQ.Enqueue(message);
        }

        public void Dispose()
        {
            processor.Dispose();
        }
    }
}
