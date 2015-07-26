namespace Asynqueue
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// QueriableAsynqueue is a class which allows a caller to queue a message and then
    /// receive and process the response to that message.
    /// </summary>
    /// <typeparam name="TIn">The type of message sent to the background process.</typeparam>
    /// <typeparam name="TOut">The type of response returned from the background process</typeparam>
    public class QueriableAsynqueue<TIn, TOut>
    {
        private Asynqueue<Asynquery<TIn, TOut>> queue;

        /// <summary>
        /// Constructs a new instance of the QueriableAsynqueue object.
        /// </summary>
        /// <param name="actorfn">The background process that receives and processes messages.</param>
        public QueriableAsynqueue(Func<TIn, TOut> actorfn)
        {
            queue = new Asynqueue<Asynquery<TIn, TOut>>(TryActorFn(req => req.Respond(actorfn(req.Input))));
        }

        /// <summary>
        /// Constructs a new instance of the QueriableAsynqueue object.
        /// </summary>
        /// <param name="actorfn">The background process that receives and processes messages.</param>
        public QueriableAsynqueue(Func<TIn, Task<TOut>> actorfn)
        {
            queue = new Asynqueue<Asynquery<TIn, TOut>>(TryActorFn(async req =>
            {
                var response = await actorfn(req.Input);
                req.Respond(response);
            }));
        }

        /// <summary>
        /// Sends a query to the background process and returns the awaitable response.
        /// </summary>
        /// <param name="input">The messagae to send to the background process.</param>
        /// <returns>The awaitable response to the message.</returns>
        public IAwaitable<TOut> Query(TIn input)
        {
            var query = new Asynquery<TIn, TOut>(input);
            queue.Send(query);
            return query;
        }
        
        private static Action<Asynquery<TIn, TOut>> TryActorFn(Action<Asynquery<TIn, TOut>> fn)
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
