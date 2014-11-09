using Microsoft.Diagnostics.Tracing;
using System.Reactive.Linq;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwStream.ConsoleViewer
{
    class Program
    {
        // TODO:Currently, sandbox.

        static ConsoleColor? GetColorMap(TraceEvent traceEvent)
        {
            switch (traceEvent.Level)
            {
                case TraceEventLevel.Critical:
                    return ConsoleColor.Magenta;
                case TraceEventLevel.Error:
                    return ConsoleColor.Red;
                case TraceEventLevel.Informational:
                    return ConsoleColor.Gray;
                case TraceEventLevel.Verbose:
                    return ConsoleColor.Green;
                case TraceEventLevel.Warning:
                    return ConsoleColor.Yellow;
                case TraceEventLevel.Always:
                    return ConsoleColor.White;
                default:
                    return null;
            }
        }

        static void Main(string[] args)
        {
            if (true)
            {
                while (true)
                {
                    Console.WriteLine("Start and Show ETW TraceSession");
                    Console.WriteLine("Press Ctrl+C : Cancel");
                    Console.Write("Enter ProviderName(or GUID): ");
                    var nameOrGuid = Console.ReadLine();
                    Console.WriteLine();

                    Guid guid;
                    var eventListener = Guid.TryParse(nameOrGuid, out guid)
                        ? ObservableEventListener.FromTraceEvent(guid)
                        : ObservableEventListener.FromTraceEvent(nameOrGuid);

                    CancellationTokenSource cts = new CancellationTokenSource();
                    var subscription = eventListener
                        .ForEachAsync(x =>
                        {
                            var currentColor = Console.ForegroundColor;
                            try
                            {
                                var eventColor = GetColorMap(x);
                                if (eventColor != null)
                                {
                                    Console.ForegroundColor = eventColor.Value;
                                }

                                var processName = default(string);
                                try
                                {
                                    var process = System.Diagnostics.Process.GetProcessById(x.ProcessID);
                                    processName = process.ProcessName;
                                }
                                catch (ArgumentException)
                                {
                                }

                                Console.WriteLine(((processName != null) ? "[" + processName + "]" : "") + x.DumpPayloadOrMessage());
                            }
                            finally
                            {
                                Console.ForegroundColor = currentColor;
                            }
                        }, cts.Token);

                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                    };

                    try
                    {
                        subscription.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerExceptions.Any(x => x is TaskCanceledException))
                        {
                            Console.WriteLine(); // restart
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }





            // Microsoft.Diagnostics.Tracing.get

            //var session = new TraceEventSession("MySource2");
            ////{
            //session.Source.Dynamic.All += delegate(TraceEvent data)              // Set Source (stream of events) from session.  
            //{                                                                    // Get dynamic parser (knows about EventSources) 
            //    // Subscribe to all EventSource events
            //    Console.WriteLine("GOT Event " + data.EventName);                          // Print each message as it comes in 
            //};

            //var eventSourceGuid = TraceEventProviders.GetEventSourceGuidFromName("MySource2"); // Get the unique ID for the eventSouce. 
            //session.EnableProvider(eventSourceGuid);                                               // Enable MyEventSource.
            //Task.Factory.StartNew(() => session.Source.Process());                                                              // Wait for incoming events (forever).  


            var actives = TraceEventSession.GetActiveSessionNames();

            var hoge = actives.Where(x => x.StartsWith("EtwStream")).ToArray();
            foreach (var item in hoge)
            {
                new TraceEventSession(item, TraceEventSessionOptions.Create).Dispose();
            }
            var hoge2 = actives.Where(x => x.StartsWith("EtwStream")).ToArray();

            //ObservableEventListener.FromTraceEvent("MySource2")
            ObservableEventListener.FromEventSource(MySource2.Log)
                .Subscribe(x =>
                {
                    Console.WriteLine(x.DumpPayloadOrMessage());
                });



            //Parallel.For(0, 10, x => { });
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    MySource2.Log.Hello(100, 200, "hogehoge!");


                    Thread.Sleep(TimeSpan.FromSeconds(100));
                }
            });

            Console.ReadLine();
        }
    }

    [EventSource(Name = "MySource2")]
    public sealed class MySource2 : Microsoft.Diagnostics.Tracing.EventSource
    {
        public static MySource2 Log = new MySource2();

        MySource2()
        {

        }

        public static class Keywords
        {
            public const EventKeywords Hoge = (EventKeywords)1;
        }

        public static class Tasks
        {
            public const EventTask TraceEventSession = (EventTask)1;
        }

        public static class Opcodes
        {
            public const EventOpcode HugaHuga = (EventOpcode)13;
        }

        [Event(1, Keywords = Keywords.Hoge, Task = Tasks.TraceEventSession, Opcode = Opcodes.HugaHuga)]
        public void Hello(int z, int y, string v)
        {
            WriteEvent(1, z, y, v ?? "");
        }

        public void Hello3(int z, int y, string v)
        {
            WriteEvent(2, z, y, v ?? "");
        }
    }

}
