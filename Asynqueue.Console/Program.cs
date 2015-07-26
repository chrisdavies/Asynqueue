namespace Asynqueue.Console
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Just a handful of tests that are too tricky to do properly as
    /// unit tests, but still matter. (Testing concurrency actually 
    /// works as expected, perf tests, etc).
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            while (true)
            {
                DemoPerfPlainQueues();
                Console.WriteLine("Press the 'x' key to exit");
                if (Console.ReadKey().KeyChar == 'x') break;
            }
        }

        private static async Task TestSingleActor()
        {
            Console.WriteLine("Single actor");
            var count = 0;
            var q = new Asynqueue<int>(i =>
            {
                if (Interlocked.Increment(ref count) > 1)
                {
                    Console.WriteLine("Count > 1");
                }

                Thread.Sleep(250);
                Interlocked.Decrement(ref count);
                Console.WriteLine("ActorDone");
            });

            for (var i = 0; i < 5; ++i)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    for (var x = 0; x < 5000; ++x)
                    {
                        q.Send(x);
                    }

                    Console.WriteLine("Done");
                });
            }
        }

        /// <summary>
        /// Demonstrate the performance of the plain messenger.
        /// </summary>
        private static async Task DemoPerfPlainQueues()
        {
            Console.WriteLine("Plain queues");
            const int NumMessages = 1000000;
            var done = new TaskCompletionSource<int>();
            var count = 0;
            Stopwatch w = Stopwatch.StartNew();

            var qout = new Asynqueue<string>(_ =>
            {
                if (++count >= NumMessages)
                {
                    Console.WriteLine("Done in " + w.ElapsedMilliseconds + "ms");
                    done.SetResult(1);
                }
            });
            
            for (var x = 0; x < NumMessages; ++x)
            {
                qout.Send("Msg " + x);
            }

            await done.Task;
        }

        /// <summary>
        /// Demonstrate the performance of the queriable messenger.
        /// </summary>
        private static async Task DemoPerfQueryQueues()
        {
            Console.WriteLine("Query queues");
            var queue = new QueriableAsynqueue<int, string>(i => "Hey " + i);

            var w = Stopwatch.StartNew();

            for (var x = 1; x < 1000000; ++x)
            {
                await queue.Query(x);
            }

            Console.WriteLine("Done in " + w.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// Demonstrate that messangers work across threads.
        /// </summary>
        private static void DemoMultithreadedness()
        {
            Console.WriteLine("Multi-threaded");
            var queue = new QueriableAsynqueue<int, string>(i => "Hey " + i);

            // Create 10 threads (more or less, depending on the ThreadPool)
            // and have each of them send queries to the queue
            for (var i = 0; i < 10; ++i)
            {
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    var originalId = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine("T" + originalId + " starting");

                    for (var x = 1; x <= 1000000; ++x)
                    {
                        var a = await queue.Query(x);
                        if (x % 100000 == 0)
                        {
                            Console.WriteLine("T" + originalId + " is now " + Thread.CurrentThread.ManagedThreadId);
                            await Task.Delay(1);
                        }
                    }

                    Console.WriteLine("T" + originalId + " stopping, (is now " + Thread.CurrentThread.ManagedThreadId + ")");
                });
            }
        }
    }
}