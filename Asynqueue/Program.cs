namespace Asynqueue
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
            DemoPerfQueryQueues();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        /// <summary>
        /// Demonstrate the performance of the plain messenger.
        /// </summary>
        private static async Task DemoPerfPlainQueues()
        {
            var qin = new Messenger<int>();
            var qout = new Messenger<string>();

            var processor = new Task(async () =>
            {
                var x = 0;
                while (true)
                {
                    var i = await qin.Receive();
                    --x;
                    qout.Send((x).ToString());
                }
            });

            processor.Start();

            var w = Stopwatch.StartNew();
                        
            for (var x = 1; x < 100000; ++x)
            {
                qin.Send(x);
                var a = await qout.Receive();
            }

            Console.WriteLine("Done in " + w.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// Demonstrate the performance of the queriable messenger.
        /// </summary>
        private static async Task DemoPerfQueryQueues()
        {
            var queue = new QueriableMessenger<int, string>();

            var processor = new Task(async () =>
            {
                while (true)
                {
                    var i = await queue.Receive();
                    i.Respond("Hey " + i.Input);
                }
            });

            processor.Start();

            var w = Stopwatch.StartNew();

            for (var x = 1; x < 100000; ++x)
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
            var queue = new QueriableMessenger<int, string>();

            // Create an actor task that will read from the queue
            // and send some processed response.
            var actor = new Task(async () =>
            {
                while (true)
                {
                    var request = await queue.Receive();
                    request.Respond("Hey " + request.Input);
                }
            });

            actor.Start();

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
                            await Task.Yield();
                        }
                    }

                    Console.WriteLine("T" + originalId + " stopping, (is now " + Thread.CurrentThread.ManagedThreadId + ")");
                });
            }
        }
    }
}