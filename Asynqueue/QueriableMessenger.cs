namespace Asynqueue
{
    public class QueriableMessenger<TIn, TOut> : Messenger<AsyncQuery<TIn, TOut>>
    {
        public AsyncQuery<TIn, TOut> Query(TIn input)
        {
            var query = new AsyncQuery<TIn, TOut>(input);
            this.Send(query);
            return query;
        }
    }
}
