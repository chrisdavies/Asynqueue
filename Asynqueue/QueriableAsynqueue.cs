namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    public class QueriableAsynqueue<TIn, TOut> : Asynqueue<AsyncQuery<TIn, TOut>>, IDisposable
    {
        private Task processor;
        private Func<TIn, Task<TOut>> actorfn;

        public QueriableAsynqueue<TIn, TOut> Actor(Func<TIn, TOut> actorfn)
        {
            return ActorFn(req => req.Respond(actorfn(req.Input)));
        }

        public QueriableAsynqueue<TIn, TOut> Actor(Func<TIn, Task<TOut>> actorfn)
        {
            return ActorFn(async req =>
            {
                var result = await actorfn(req.Input);
                req.Respond(result);
            });
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

        private QueriableAsynqueue<TIn, TOut> ActorFn(Action<AsyncQuery<TIn, TOut>> fn)
        {
            base.Actor(req =>
            {
                try
                {
                    fn(req);
                }
                catch (Exception ex)
                {
                    req.Respond(ex);
                }
            });

            return this;
        }
    }
}
