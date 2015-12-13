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
            var sink = new TraceEventSink(x => x.EventName + ": " + x.DumpPayloadOrMessage());
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            var sink = new TraceEventSink(messageFormatter);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<IList<TraceEvent>> source)
        {
            var sink = new TraceEventSink(x => x.EventName + ": " + x.DumpPayloadOrMessage());
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<IList<TraceEvent>> source, Func<TraceEvent, string> messageFormatter)
        {
            var sink = new TraceEventSink(messageFormatter);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // EventArgs

        public static IDisposable LogToConsole(this IObservable<EventWrittenEventArgs> source)
        {
            var sink = new EventWrittenEventArgsSink(x => x.EventName + ": " + x.DumpPayloadOrMessage());
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<EventWrittenEventArgs> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            var sink = new EventWrittenEventArgsSink(messageFormatter);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<IList<EventWrittenEventArgs>> source)
        {
            var sink = new EventWrittenEventArgsSink(x => x.EventName + ": " + x.DumpPayloadOrMessage());
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToConsole(this IObservable<IList<EventWrittenEventArgs>> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            var sink = new EventWrittenEventArgsSink(messageFormatter);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // String

        public static IDisposable LogToConsole(this IObservable<string> source)
        {
            return source.Subscribe(x => Console.WriteLine(x));
        }

        public static IDisposable LogToConsole(this IObservable<IList<string>> source)
        {
            return source.Subscribe(x => Console.WriteLine(x));
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

            public override void Dispose()
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
                            EtwStreamEventSource.Log.SinkError(nameof(ConsoleSink), "messageFormatter convert failed", ex.ToString());
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

            public override void Dispose()
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
                            EtwStreamEventSource.Log.SinkError(nameof(ConsoleSink), "messageFormatter convert failed", ex.ToString());
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