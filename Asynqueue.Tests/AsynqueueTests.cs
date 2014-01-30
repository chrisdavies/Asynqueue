namespace Asynqueue.Tests
{
    using Asynqueue;
    using Should;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class AsynqueueTests
    {
        [Fact]
        public void Asynqueue_runs_when_queued()
        {
            var response = new TaskCompletionSource<string>();
            var q = new Asynqueue<string>(s => response.SetResult("Hello, " + s));
            q.Send("World");

            response.Task.Wait();
            var result = response.Task.Result;
            "Hello, World".ShouldEqual(result);
        }

        [Fact]
        public void Asynqueue_send_is_async()
        {
            var response = new TaskCompletionSource<int>();
            var count = 0;

            var q = new Asynqueue<int>(async i =>
            {
                await Task.Delay(10);
                response.SetResult(Interlocked.Increment(ref count));
            });
         
            q.Send(1);
            Interlocked.Increment(ref count).ShouldEqual(1);
            response.Task.Wait();
            response.Task.Result.ShouldEqual(2);
        }
    }
}
