namespace Asynqueue
{
    using System;
    using System.Runtime.CompilerServices;

    public class AsyncQuery<TIn, TOut> : IAwaitable<TOut>
    {
        private TOut response;
        private Exception exception;
        private Action continuation;

        public bool IsCompleted { get; set; }

        public TIn Input { get; set; }

        public AsyncQuery(TIn input)
        {
            this.Input = input;
        }

        public void Respond(Exception ex)
        {
            Respond(() => this.exception = ex);
        }

        public void Respond(TOut response)
        {
            Respond(() => this.response = response);
        }

        public IAwaitable<TOut> GetAwaiter()
        {
            return this;
        }

        public void OnCompleted(Action continuation)
        {
            // Locking on self. Don't care if it's bad form.
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
            if (exception != null) throw exception;

            return response;
        }

        private void Respond(Action fn)
        {
            lock (this)
            {
                fn();
                this.IsCompleted = true;

                if (this.continuation != null)
                {
                    this.continuation();
                }
            }
        }
    }
}
