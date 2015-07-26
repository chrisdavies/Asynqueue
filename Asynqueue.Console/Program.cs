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
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
            }
            else
            {
                Task.WaitAll(RunCommand(args[0]));
            }            
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Asynqueue.Console plain");
            Console.WriteLine("Asynqueue.Console bidirectional");
        }

        private static async Task RunCommand(string cmd)
        {
            switch (cmd.ToLowerInvariant())
            {
                case "plain":
                    await AveragePerf(PlainQueue);
                    break;
                case "bidirectional":
                    await AveragePerf(BidirectionalQueue);
                    break;
                default:
                    PrintHelp();
                    break;
            }
        }

        public static async Task AveragePerf(Func<Task<long>> fn)
        {
            const int passes = 10;
            long ms = 0;

            for (var i = 0; i < passes; ++i)
            {
                var fnMs = await fn();
                ms += fnMs;
                Console.WriteLine(fnMs + "ms");
            }

            Console.WriteLine($"\n {ms / passes}ms avg");
        }

        /// <summary>
        /// Demonstrate the performance of the plain messenger.
        /// </summary>
        private static async Task<long> PlainQueue()
        {
            const int NumMessages = 1000000;
            var done = new TaskCompletionSource<long>();
            var count = 0;
            Stopwatch w = Stopwatch.StartNew();

            var qout = new Asynqueue<string>(_ =>
            {
                if (++count >= NumMessages)
                {
                    done.SetResult(w.ElapsedMilliseconds);
                }
            });
            
            for (var x = 0; x < NumMessages; ++x)
            {
                qout.Send("Msg " + x);
            }

            return await done.Task;
        }

        /// <summary>
        /// Demonstrate the performance of the queriable messenger.
        /// </summary>
        private static async Task<long> BidirectionalQueue()
        {
            var queue = new QueriableAsynqueue<int, string>(i => "Hey " + i);

            var w = Stopwatch.StartNew();

            for (var x = 1; x < 1000000; ++x)
            {
                await queue.Query(x);
            }

            return w.ElapsedMilliseconds;
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