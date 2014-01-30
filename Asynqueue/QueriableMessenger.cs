namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    public class QueriableMessenger<TIn, TOut> : Messenger<AsyncQuery<TIn, TOut>>, IDisposable
    {
        private Task processor;

        public QueriableMessenger<TIn, TOut> Actor(Func<TIn, TOut> actor)
        {
            return this.Actor(i => Task.FromResult(actor(i)));
        }

        public QueriableMessenger<TIn, TOut> Actor(Func<TIn, Task<TOut>> actor)
        {
            Sync(() =>
            {
                if (this.processor != null)
                {
                    throw new InvalidOperationException("Only one actor can be associated with a queue.");
                }

                this.processor = new Task(async () =>
                {
                    while (true)
                    {
                        var request = await Receive();
                        try
                        {
                            var response = await actor(request.Input);
                            request.Respond(response);
                        }
                        catch (Exception ex)
                        {
                            request.Respond(ex);
                        }
                    }
                });
            });

            this.processor.Start();
            return this;
        }

        public AsyncQuery<TIn, TOut> Query(TIn input)
        {
            var query = new AsyncQuery<TIn, TOut>(input);
            this.Send(query);
            return query;
        }

        public void Dispose()
        {
            if (this.processor != null)
            {
                this.processor.Dispose();
                this.processor = null;
            }
        }
    }
}
