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

#if TRACE_EVENT
using Microsoft.Diagnostics.Tracing;
#endif

namespace EtwStream
{
    public static class DebugSink
    {
        // TraceEvent

#if TRACE_EVENT

        public static IDisposable LogToDebug(this IObservable<TraceEvent> source)
        {
            return source.Subscribe(x => Debug.WriteLine(x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToDebug(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            return source.Subscribe(x => Debug.WriteLine(messageFormatter(x)));
        }

#endif

        // EventArgs

        public static IDisposable LogToDebug(this IObservable<EventWrittenEventArgs> source)
        {
            return source.Subscribe(x => Debug.WriteLine(x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToDebug(this IObservable<EventWrittenEventArgs> source, Func<EventWrittenEventArgs, string> messageFormatter)
        {
            return source.Subscribe(x => Debug.WriteLine(messageFormatter(x)));
        }

        // String

        public static IDisposable LogToDebug(this IObservable<string> source)
        {
            return source.Subscribe(x => Debug.WriteLine(x));
        }
    }
}