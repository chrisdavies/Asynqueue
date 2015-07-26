namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynqueue is a class that behaves similarly to golang's channels. It provides a simple,
    /// lightweight, and fast mechanism for sending messages to a background processing function.
    /// </summary>
    /// <typeparam name="T">The type of message to be sent</typeparam>
    public class Asynqueue<T> : IDisposable
    {
        private Task processor;
        private AsynqueueAwaitable<T> awaitableQ = new AsynqueueAwaitable<T>();

        /// <summary>
        /// Constructs a new instance of Asynqueue.
        /// </summary>
        /// <param name="handler">The action which handles messages in a background thread/task</param>
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

        /// <summary>
        /// Send sends a message to the background process.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void Send(T message)
        {
            awaitableQ.Enqueue(message);
        }

        /// <summary>
        /// Dispose cleans up the asynqueue class.
        /// </summary>
        public void Dispose()
        {
            processor.Dispose();
        }
    }
}
