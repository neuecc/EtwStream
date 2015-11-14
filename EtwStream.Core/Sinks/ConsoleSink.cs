using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace EtwStream
{
    public static class ConsoleSink
    {
        // TraceEvent

        public static IDisposable LogToConsole(this IObservable<TraceEvent> source)
        {
            return source.Subscribe(new TraceEventSink(x => x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToConsole(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            return source.Subscribe(new TraceEventSink(messageFormatter));
        }

        public static Task LogToConsoleAsync(this IObservable<TraceEvent> source)
        {
            return source.Do(new TraceEventSink(x => x.EventName + ": " + x.DumpPayloadOrMessage())).ToTask();
        }

        public static Task LogToConsoleAsync(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            return source.Do(new TraceEventSink(messageFormatter)).ToTask();
        }

        // EventArgs

        public static IDisposable LogToConsole(this IObservable<EventWrittenEventArgs> source)
        {
            return source.Subscribe(new EventWrittenEventArgsSink(x => x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToConsole(this IObservable<EventWrittenEventArgs> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            return source.Subscribe(new EventWrittenEventArgsSink(messageFormatter));
        }

        public static Task LogToConsoleAsync(this IObservable<EventWrittenEventArgs> source)
        {
            return source.Do(new EventWrittenEventArgsSink(x => x.EventName + ": " + x.DumpPayloadOrMessage())).ToTask();
        }

        public static Task LogToConsoleAsync(this IObservable<EventWrittenEventArgs> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            return source.Do(new EventWrittenEventArgsSink(messageFormatter)).ToTask();
        }

        // String

        public static IDisposable LogToConsole(this IObservable<string> source)
        {
            return source.Subscribe(x => Console.WriteLine(x));
        }

        public static Task LogToConsoleAsync(this IObservable<string> source)
        {
            return source.Do(x => Console.WriteLine(x)).ToTask();
        }

        // Sinks

        class TraceEventSink : SinkBase<TraceEvent>
        {
            static readonly object consoleColorChangeLock = new object();
            readonly Func<TraceEvent, string> messageFormatter;

            public TraceEventSink(Func<TraceEvent, string> messageFormatter)
            {
                this.messageFormatter = messageFormatter;
            }

            public override void Flush()
            {
                // do nothing
            }

            public override void OnNext(IList<TraceEvent> value)
            {
                foreach (var item in value)
                {
                    lock (consoleColorChangeLock)
                    {
                        var currentColor = Console.ForegroundColor;
                        try
                        {
                            var color = item.GetColorMap(isBackgroundWhite: false);
                            if (color != null)
                            {
                                Console.ForegroundColor = color.Value;
                            }
                            Console.WriteLine(messageFormatter(item));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            Console.ForegroundColor = currentColor;
                        }
                    }
                }
            }
        }

        class EventWrittenEventArgsSink : SinkBase<EventWrittenEventArgs>
        {
            static readonly object consoleColorChangeLock = new object();
            readonly Func<EventWrittenEventArgs, string> messageFormatter;

            public EventWrittenEventArgsSink(Func<EventWrittenEventArgs, string> messageFormatter)
            {
                this.messageFormatter = messageFormatter;
            }

            public override void Flush()
            {
                // do nothing
            }

            public override void OnNext(IList<EventWrittenEventArgs> value)
            {
                foreach (var item in value)
                {
                    lock (consoleColorChangeLock)
                    {
                        var currentColor = Console.ForegroundColor;
                        try
                        {
                            var color = item.GetColorMap(isBackgroundWhite: false);
                            if (color != null)
                            {
                                Console.ForegroundColor = color.Value;
                            }
                            Console.WriteLine(messageFormatter(item));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            Console.ForegroundColor = currentColor;
                        }
                    }
                }
            }
        }
    }
}