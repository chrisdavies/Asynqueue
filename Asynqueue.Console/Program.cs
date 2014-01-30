namespace Asynqueue.Console
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            while (true)
            {
                DemoPerfQueryQueues();
                Console.WriteLine("Press the 'x' key to exit");
                if (Console.ReadKey().KeyChar == 'x') break;
            }
            
        }

        private static async Task DemoException()
        {
            var queue = new QueriableAsynqueue<int, string>()
                .Actor(i =>
                {
                    if (i > 10)
                    {
                        return "Good to go";
                    }

                    throw new IndexOutOfRangeException("Needs to be greater than 10");
                });

            try
            {
                var result = await queue.Query(1232);
                Console.WriteLine(result);
                result = await queue.Query(9);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERR: " + ex.Message);
            }
        }

        /// <summary>
        /// Demonstrate that send is really async.
        /// </summary>
        private static void DemoAsyncSend()
        {
            var queue = new Asynqueue<int>()
                .Actor(async i => {
                    await Task.Delay(1000);
                    Console.WriteLine("Got " + i);
                });

            queue.Send(1);
            Console.WriteLine("All sent and stuff...");
        }

        /// <summary>
        /// Demonstrate the performance of the plain messenger.
        /// </summary>
        private static async Task DemoPerfPlainQueues()
        {
            const int NumMessages = 1000000;
            var done = new TaskCompletionSource<int>();
            var qout = new Asynqueue<string>();
            var qin = new Asynqueue<int>()
                .Actor(i => qout.Send("Msg " + i));
            var w = Stopwatch.StartNew();

            for (var x = 0; x < NumMessages; ++x)
            {
                qin.Send(x);
            }

            var count = 0;
            qout.Actor(_ =>
            {
                if (++count >= NumMessages)
                {
                    Console.WriteLine("Done in " + w.ElapsedMilliseconds + "ms");
                    done.SetResult(1);
                }
            });

            await done.Task;
        }

        /// <summary>
        /// Demonstrate the performance of the queriable messenger.
        /// </summary>
        private static async Task DemoPerfQueryQueues()
        {
            var queue = new QueriableAsynqueue<int, string>()
                .Actor(i => "Hey " + i);

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
            var queue = new QueriableAsynqueue<int, string>()
                .Actor(i => "Hey " + i);

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