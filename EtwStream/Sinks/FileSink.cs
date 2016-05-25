using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Diagnostics.Tracing;
#if TRACE_EVENT
using Microsoft.Diagnostics.Tracing;
#endif

namespace EtwStream
{
    public static class FileSink
    {
        // TraceEvent

#if TRACE_EVENT

        public static IDisposable LogToFile(this IObservable<TraceEvent> source, string fileName, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileName, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToFile(this IObservable<IList<TraceEvent>> source, string fileName, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileName, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

#endif

        // EventArgs

        public static IDisposable LogToFile(this IObservable<EventWrittenEventArgs> source, string fileName, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileName, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToFile(this IObservable<IList<EventWrittenEventArgs>> source, string fileName, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileName, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // string

        public static IDisposable LogToFile(this IObservable<string> source, string fileName, Encoding encoding, bool autoFlush)
        {
            var sink = new StringSink(fileName, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToFile(this IObservable<IList<string>> source, string fileName, Encoding encoding, bool autoFlush)
        {
            var sink = new StringSink(fileName, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // Sinks

#if TRACE_EVENT

        class TraceEventSink : SinkBase<TraceEvent>
        {
            readonly Func<TraceEvent, string> messageFormatter;
            readonly AsyncFileWriter asyncFileWriter;

            public TraceEventSink(string fileName, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
            {
                this.asyncFileWriter = new AsyncFileWriter(nameof(FileSink), fileName, encoding, autoFlush);
                this.messageFormatter = messageFormatter;
            }

            public override void OnNext(TraceEvent value)
            {
                string v;
                try
                {
                    v = messageFormatter(value);
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(FileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }
                asyncFileWriter.Enqueue(v);
            }

            public override void OnNext(IList<TraceEvent> value)
            {
                string v;
                try
                {
                    v = string.Join(Environment.NewLine, value.Select(x => messageFormatter(x)));
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(FileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }
                asyncFileWriter.Enqueue(v);
            }

            public override void Dispose()
            {
                asyncFileWriter.Finalize();
            }
        }

#endif

        class EventWrittenEventArgsSink : SinkBase<EventWrittenEventArgs>
        {
            readonly Func<EventWrittenEventArgs, string> messageFormatter;
            readonly AsyncFileWriter asyncFileWriter;

            public EventWrittenEventArgsSink(string fileName, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
            {
                this.asyncFileWriter = new AsyncFileWriter(nameof(FileSink), fileName, encoding, autoFlush);
                this.messageFormatter = messageFormatter;
            }

            public override void OnNext(EventWrittenEventArgs value)
            {
                string v;
                try
                {
                    v = messageFormatter(value);
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(FileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }

                asyncFileWriter.Enqueue(v);
            }

            public override void OnNext(IList<EventWrittenEventArgs> value)
            {
                string v;
                try
                {
                    v = string.Join(Environment.NewLine, value.Select(x => messageFormatter(x)));
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(FileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }
                asyncFileWriter.Enqueue(v);
            }

            public override void Dispose()
            {
                asyncFileWriter.Finalize();
            }
        }

        class StringSink : SinkBase<string>
        {
            readonly AsyncFileWriter asyncFileWriter;

            public StringSink(string fileName, Encoding encoding, bool autoFlush)
            {
                this.asyncFileWriter = new AsyncFileWriter(nameof(FileSink), fileName, encoding, autoFlush);
            }

            public override void OnNext(string value)
            {
                asyncFileWriter.Enqueue(value);
            }

            public override void OnNext(IList<string> value)
            {
                string v = string.Join(Environment.NewLine, value);
                asyncFileWriter.Enqueue(v);
            }

            public override void Dispose()
            {
                asyncFileWriter.Finalize();
            }
        }
    }
}