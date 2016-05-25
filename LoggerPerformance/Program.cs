using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtwStream;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;
using Serilog;
using Serilog.Sinks;
using System.Reactive.Linq;

namespace LoggerPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            //EtwStream.RollCheck();

            EtwStream.Test2();


            //Console.WriteLine("EtwStream");
            //EtwStream.Run();

            //Console.WriteLine("NLog");
            //NLoog.Run();

            //Console.WriteLine("Slab");
            //Slab.Run();


            //Console.WriteLine("Serilooog");
            //Serilooog.Run();
            ////EtwStream.Test();
        }

        static class EtwStream
        {
            private static readonly MyEventSource loggger = MyEventSource.Log;

            public static void Run()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var cts = new CancellationTokenSource();

                var subscription = ObservableEventListener.FromEventSource(MyEventSource.Log)
                    .Buffer(TimeSpan.FromSeconds(5), 1000, cts.Token)
                    //.TakeUntil(cts.Token)
                    .LogToFile("mytest.txt", x => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss,fff") + " " + "Info " + x.Payload[0], Encoding.UTF8, false);
                //.LogToRollingFlatFile((dt, count) => $"test{count}.txt", dt => "", 4000,
                //                  x => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss,fff") + " " + "Info " + x.Payload[0], Encoding.UTF8, false);

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    loggger.Info(Guid.NewGuid().ToString());
                }
                Console.WriteLine("come here:" + sw.Elapsed.TotalMilliseconds);

                cts.Cancel();
                subscription.Dispose();

                Console.WriteLine("after wait:" + sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                Console.WriteLine("time: " + sw.Elapsed.TotalMilliseconds + "ms");
                // line / elapsed
                Console.WriteLine(((double)100000 / sw.Elapsed.TotalMilliseconds) + "ms");
            }

            public static void Test()
            {
                var cts = new CancellationTokenSource();

                var subscription = ObservableEventListener.FromEventSource(MyEventSource.Log)
                    .Buffer(TimeSpan.FromSeconds(5), 1000, cts.Token)
                    .LogToFile("etw.txt", x => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss,fff") + " " + "Info " + x.Payload[0], Encoding.UTF8, true);

                loggger.Info("aiueo");
                loggger.Info("kakikukeko");

                Console.WriteLine("waiting...");
                Console.ReadLine();
                cts.Cancel();
                subscription.Dispose();
            }

            public static void Test2()
            {
                var cts = new CancellationTokenSource();

                var subscription = ObservableEventListener.FromEventSource(MyEventSource.Log)
                    .Buffer(TimeSpan.FromSeconds(5), 1000, cts.Token)
                    .LogToFile("etw.txt", x => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss,fff") + " " + "Info " + x.Payload[0], Encoding.UTF8, true);

                Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(8))
                    .Subscribe(_ =>
                    {
                        loggger.Info("hogehogehogehoge");
                    });



                Console.WriteLine("waiting...");
                Console.ReadLine();
                cts.Cancel();
                subscription.Dispose();
            }

            public static void RollCheck()
            {
                var cts = new CancellationTokenSource();
                var d = ObservableEventListener.FromEventSource(MyEventSource.Log)
                    .Buffer(TimeSpan.FromSeconds(5), 1000, cts.Token)
                    //.LogToFile("hoge.txt", x => (string)x.Payload[0], Encoding.UTF8, false);
                    .LogToRollingFile((dt, i) => $@"EtwStreamLog\RollingCheck{dt.ToString("yyyyMMdd")}-{i}.log", x => x.ToString("yyyyMMdd"), 10000, x => x.DumpPayloadOrMessage(), Encoding.UTF8, true);
                var sw = new Stopwatch();
                sw.Start();
                Task.WhenAll(Enumerable.Range(0, 100)
                    .Select(async (i) =>
                    {
                        foreach (var j in Enumerable.Range(0, 10000))
                        {
                            await Task.Run(() =>
                            {
                                MyEventSource.Log.Info($"abc{i}:{j}");
                            });
                        }
                    }))
                    .Wait();

                cts.Cancel();
                d.Dispose();

                sw.Stop();
                Console.WriteLine("elapsed {0}", sw.Elapsed);
            }
        }

        static class Slab
        {
            private static readonly MyEventSource Loggger = MyEventSource.Log;

            public static void Run()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var l = new Microsoft.Practices.EnterpriseLibrary.SemanticLogging.ObservableEventListener();
                l.EnableEvents(MyEventSource.Log, EventLevel.Informational);
                var subscription = Microsoft.Practices.EnterpriseLibrary.SemanticLogging.FlatFileLog.LogToFlatFile(l, "slab.txt", isAsync: true);

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    Loggger.Info(Guid.NewGuid().ToString());
                }
                Console.WriteLine("come here:" + sw.Elapsed.TotalMilliseconds);

                subscription.Dispose();

                Console.WriteLine("after wait:" + sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                Console.WriteLine("time: " + sw.Elapsed.TotalMilliseconds + "ms");
                // line / elapsed
                Console.WriteLine(((double)100000 / sw.Elapsed.TotalMilliseconds) + "ms");
            }
        }

        static class NLoog
        {
            private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            public static void Run()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    logger.Info(Guid.NewGuid().ToString());
                }
                Console.WriteLine("come here:" + sw.Elapsed.TotalMilliseconds);

                Console.WriteLine("after wait:" + sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                Console.WriteLine("time: " + sw.Elapsed.TotalMilliseconds + "ms");
                // line / elapsed
                Console.WriteLine(((double)100000 / sw.Elapsed.TotalMilliseconds) + "ms");
            }
        }

        static class Serilooog
        {
            public static void Run()
            {
                var logger = new Serilog.LoggerConfiguration()

                    .WriteTo.File("seri.txt").CreateLogger();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    logger.Information(Guid.NewGuid().ToString());
                }
                Console.WriteLine("come here:" + sw.Elapsed.TotalMilliseconds);

                Console.WriteLine("after wait:" + sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                Console.WriteLine("time: " + sw.Elapsed.TotalMilliseconds + "ms");
                // line / elapsed
                Console.WriteLine(((double)100000 / sw.Elapsed.TotalMilliseconds) + "ms");

            }
        }
    }

    [EventSource(Name = "MyEventSource")]
    public class MyEventSource : EventSource
    {
        public static MyEventSource Log = new MyEventSource();

        MyEventSource()
        {

        }

        [Event(1)]
        public void Info(string msg)
        {
            WriteEvent(1, msg ?? "");
        }
    }
}
