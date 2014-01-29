namespace Asynqueue
{
    using System;
    using System.Runtime.CompilerServices;

    public class AsyncQuery<TIn, TOut> : INotifyCompletion
    {
        private TOut response;
        private Action continuation;

        public bool IsCompleted { get; set; }

        public TIn Input { get; set; }

        public AsyncQuery(TIn input)
        {
            this.Input = input;
        }

        public void Respond(TOut response)
        {
            lock (this)
            {
                this.response = response;
                this.IsCompleted = true;

                if (this.continuation != null)
                {
                    this.continuation();
                }
            }
        }

        public AsyncQuery<TIn, TOut> GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            lock (this)
            {
                this.continuation = continuation;
                if (this.IsCompleted)
                {
                    this.continuation();
                }
            }
        }

        public TOut GetResult()
        {
            return response;
        }
    }
}
