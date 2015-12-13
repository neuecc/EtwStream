using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static class TraceSink
    {
        // TraceEvent

        public static IDisposable LogToTrace(this IObservable<TraceEvent> source)
        {
            return source.Subscribe(x => Trace.WriteLine(x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToTrace(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            return source.Subscribe(x => Trace.WriteLine(messageFormatter(x)));
        }

        // EventArgs

        public static IDisposable LogToTrace(this IObservable<EventWrittenEventArgs> source)
        {
            return source.Subscribe(x => Trace.WriteLine(x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToTrace(this IObservable<EventWrittenEventArgs> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            return source.Subscribe(x => Trace.WriteLine(messageFormatter(x)));
        }

        // String

        public static IDisposable LogToTrace(this IObservable<string> source)
        {
            return source.Subscribe(x => Trace.WriteLine(x));
        }

        public static Task LogToTraceAsync(this IObservable<string> source)
        {
            return source.Do(x => Trace.WriteLine(x)).DefaultIfEmpty().ToTask();
        }
    }
}