using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using LINQPad;
using Microsoft.Diagnostics.Tracing;

namespace EtwStream
{
    public static class LinqPadExtensions
    {
        // TraceEvent

        static object WithColorStyle(object o, TraceEvent traceEvent)
        {
            var color = traceEvent.GetColorMap(isBackgroundWhite: true);
            if (color == null) return o;

            return Util.WithStyle(o, "color:" + color.ToString());
        }

        public static IObservable<object> WithColor(this IObservable<TraceEvent> source, bool withProviderName = false)
        {
            var newSource = source.Select(x =>
            {
                var message = x.DumpPayloadOrMessage();

                if (withProviderName)
                {
                    return (object)new
                    {
                        Timestamp = WithColorStyle(x.TimeStamp, x),
                        ProviderName = WithColorStyle(x.ProviderName, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
                else
                {
                    return (object)new
                    {
                        Timestamp = WithColorStyle(x.TimeStamp, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
            });

            var connectable = newSource.Replay();
            var disposable = connectable.Connect();

            // TODO:no needs dispose?
            Util.Cleanup += (sender, e) =>
            {
                disposable.Dispose(); // finish
            };

            return newSource;
        }

        public static IObservable<object> DumpToColor(this IObservable<TraceEvent> source, bool withProviderName = false)
        {
            return LINQPad.Extensions.Dump(WithColor(source, withProviderName));
        }

        // EventWrittenEventArgs

        static object WithColorStyle(object o, EventWrittenEventArgs eventArgs)
        {
            var color = eventArgs.GetColorMap(isBackgroundWhite: true);
            if (color == null) return o;

            return Util.WithStyle(o, "color:" + color.ToString());
        }

        public static IObservable<object> WithColor(this IObservable<EventWrittenEventArgs> source, bool withProviderName = false)
        {
            var newSource = source.Select(x =>
            {
                var timestamp = DateTime.Now;
                var message = x.DumpPayloadOrMessage();
                
                if (withProviderName)
                {
                    return (object)new
                    {
                        Timestamp = WithColorStyle(timestamp, x),
                        ProviderName = WithColorStyle(x.EventSource.Name, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
                else
                {
                    return (object)new
                    {
                        Timestamp = WithColorStyle(timestamp, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
            });

            var connectable = newSource.Replay();
            var disposable = connectable.Connect();

            // TODO:no needs dispose?
            Util.Cleanup += (sender, e) =>
            {
                disposable.Dispose(); // finish
            };

            return newSource;
        }

        public static IObservable<object> DumpToColor(this IObservable<EventWrittenEventArgs> source, bool withProviderName = false)
        {
            return LINQPad.Extensions.Dump(WithColor(source, withProviderName));
        }
    }
}