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
    public static class DebugSink
    {
        // TraceEvent

        public static IDisposable LogToDebug(this IObservable<TraceEvent> source)
        {
            return source.Subscribe(x => Debug.WriteLine(x.EventName + ": " + x.DumpPayloadOrMessage()));
        }

        public static IDisposable LogToDebug(this IObservable<TraceEvent> source, Func<TraceEvent, string> messageFormatter)
        {
            return source.Subscribe(x => Debug.WriteLine(messageFormatter(x)));
        }

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