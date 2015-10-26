using Microsoft.Diagnostics.Tracing;
using System.Reactive.Linq;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace EtwStream.ConsoleViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            while (true)
            {
                Console.WriteLine("Start and Show ETW TraceSession");
                Console.WriteLine("Press Ctrl+C as Cancel");
                Console.WriteLine("Enter PrividerName(e.g.'MyEventSource') or ProviderGUID(e.g.'2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5')");
                Console.WriteLine("Enter '-clr eventName*' for ClrTrace(e.g.-clr GC/Stop)");
                Console.WriteLine("Enter '-kernel flags*' for KernelTrace(e.g.-kernel DiskIO FileIO)");
                Console.WriteLine();
                Console.Write("Enter ProviderName(or GUID): ");
                var nameOrGuid = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrWhiteSpace(nameOrGuid)) continue;

                IObservable<TraceEvent> eventListener;

                var splitted = (nameOrGuid ?? "").Split(' ');
                if (nameOrGuid == "-clr" || (splitted.Length >= 1 && splitted[0] == "-clr"))
                {
                    eventListener = ObservableEventListener.FromClrTraceEvent();
                    if (splitted.Length >= 2)
                    {
                        var filter = new HashSet<string>(splitted.Skip(1));
                        eventListener = eventListener.Where(x => filter.Contains(x.EventName));
                    }
                }
                else if (splitted.Length >= 1 && splitted[0] == "-kernel")
                {
                    KernelTraceEventParser.Keywords flags;
                    if (splitted.Length == 1)
                    {
                        flags = KernelTraceEventParser.Keywords.All;
                    }
                    else
                    {
                        flags = splitted.Skip(1).Select(x => (KernelTraceEventParser.Keywords)Enum.Parse(typeof(KernelTraceEventParser.Keywords), x, true))
                            .Aggregate((x, y) => x | y);
                    }
                    eventListener = ObservableEventListener.FromKernelTraceEvent(flags);
                }
                else
                {
                    Guid guid;
                    eventListener = Guid.TryParse(nameOrGuid, out guid)
                       ? ObservableEventListener.FromTraceEvent(guid)
                       : ObservableEventListener.FromTraceEvent(nameOrGuid);
                }

                cts = new CancellationTokenSource();
                var subscription = eventListener
                    .ForEachAsync(x =>
                    {
                        var currentColor = Console.ForegroundColor;
                        try
                        {
                            var eventColor = x.GetColorMap(isBackgroundWhite: false);
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
    }
}