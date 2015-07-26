namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    public class QueriableAsynqueue<TIn, TOut> : Asynqueue<AsyncQuery<TIn, TOut>>
    {
        public QueriableAsynqueue(Func<TIn, TOut> actorfn)
            : base(TryActorFn(req => req.Respond(actorfn(req.Input))))
        {
        }

        public QueriableAsynqueue(Func<TIn, Task<TOut>> actorfn)
            : base(TryActorFn(async req => {
                var response = await actorfn(req.Input);
                req.Respond(response);
            }))
        {
        }

        public AsyncQuery<TIn, TOut> Query(TIn input)
        {
            var query = new AsyncQuery<TIn, TOut>(input);
            this.Send(query);
            return query;
        }
        
        private static Action<AsyncQuery<TIn, TOut>> TryActorFn(Action<AsyncQuery<TIn, TOut>> fn)
        {
            return (req =>
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
        }
    }
}
