namespace Asynqueue.Tests
{
    using Asynqueue;
    using Should;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class QueriableAsynqueueTests
    {
        [Fact]
        public void Queriable_queue_returns_value_to_sender()
        {
            var q = new QueriableAsynqueue<int, string>(i => "Hello " + i);

            var task = Task.Run(async () =>
            {
                var result = await q.Query(7);
                result.ShouldEqual("Hello 7");
            });
            
            task.Wait();
        }

        [Fact]
        public void Queriable_queue_throws_exception_on_sender()
        {
            Func<int, string> f = (i) => { throw new Exception("Ruh roh"); };
            var q = new QueriableAsynqueue<int, string>(f);

            var task = Task.Run(async () =>
            {
                try {
                    var result = await q.Query(7);
                }
                catch (Exception ex) {
                    ex.Message.ShouldEqual("Ruh roh");
                }
            });

            task.Wait();
        }

        [Fact]
        public void Queriable_queue_handles_multiple_sends()
        {
            var q = new QueriableAsynqueue<int, string>(i => "Hello " + i);

            var t1 = Task.Run(async () =>
            {
                var result = await q.Query(1);
                result.ShouldEqual("Hello 1");
            });

            var t2 = Task.Run(async () =>
            {
                var result = await q.Query(2);
                result.ShouldEqual("Hello 2");
            });

            Task.WaitAll(t1, t2);
        }
    }
}
