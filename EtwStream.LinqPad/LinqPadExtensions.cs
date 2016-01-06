using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Caching;
using System.Text;
using LINQPad;
using Microsoft.Diagnostics.Tracing;

namespace EtwStream
{
    public static class LinqPadExtensions
    {
        // TraceEvent

        static MemoryCache processNameCache = new MemoryCache("LinqPadProcessNameCache");

        static string GetProcessName(int processId)
        {
            var processName = processNameCache.Get(processId.ToString());

            if (processName != null) return (string)processName;
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                processName = process.ProcessName;
                processNameCache.Add(processId.ToString(), processName, DateTimeOffset.Now.AddSeconds(5));
            }
            catch
            {
            }
            return "";
        }

        static object WithColorStyle(object o, TraceEvent traceEvent)
        {
            var color = traceEvent.GetColorMap(isBackgroundWhite: true);
            if (color == null) return o;

            return Util.WithStyle(o, "color:" + color.ToString());
        }

        public static IObservable<object> WithColor(this IObservable<TraceEvent> source, bool withProviderName = false, bool withProcessName = false, Func<TraceEvent, string> messageSelector = null)
        {
            Util.AutoScrollResults = true; // force autoscroll on

            var newSource = source.Select(x =>
            {
                var message = (messageSelector == null)
                    ? x.DumpPayloadOrMessage()
                    : messageSelector(x);

                if (withProcessName && withProviderName)
                {
                    // ignore LINQPad self
                    var processName = GetProcessName(x.ProcessID);
                    if (processName == "LINQPad" || processName == "LINQPad.UserQuery") return null;

                    return (object)new
                    {
                        Timestamp = WithColorStyle(x.TimeStamp, x),
                        ProcessName = WithColorStyle(processName, x),
                        ProviderName = WithColorStyle(x.ProviderName, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
                else if (withProcessName)
                {
                    var processName = GetProcessName(x.ProcessID);
                    if (processName == "LINQPad" || processName == "LINQPad.UserQuery") return null;

                    return (object)new
                    {
                        Timestamp = WithColorStyle(x.TimeStamp, x),
                        ProcessName = WithColorStyle(processName, x),
                        EventName = WithColorStyle(x.EventName, x),
                        Message = WithColorStyle(message, x)
                    };
                }
                else if (withProviderName)
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
            })
            .Where(x => x != null);

            var connectable = newSource.Replay();
            var disposable = connectable.Connect();

            Util.Cleanup += (sender, e) =>
            {
                disposable.Dispose(); // finish
            };

            return connectable.AsObservable();
        }

        public static IObservable<object> DumpWithColor(this IObservable<TraceEvent> source, bool withProviderName = false, bool withProcessName = false, Func<TraceEvent, string> messageSelector = null)
        {
            return LINQPad.Extensions.Dump(WithColor(source, withProviderName, withProcessName, messageSelector));
        }

        // EventWrittenEventArgs

        static object WithColorStyle(object o, EventWrittenEventArgs eventArgs)
        {
            var color = eventArgs.GetColorMap(isBackgroundWhite: true);
            if (color == null) return o;

            return Util.WithStyle(o, "color:" + color.ToString());
        }

        public static IObservable<object> WithColor(this IObservable<EventWrittenEventArgs> source, bool withProviderName = false, Func<EventWrittenEventArgs, string> messageSelector = null)
        {
            Util.AutoScrollResults = true; // force autoscroll on

            var newSource = source.Select(x =>
            {
                var timestamp = DateTime.Now;
                var message = (messageSelector == null)
                    ? x.DumpPayloadOrMessage()
                    : messageSelector(x);

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

            Util.Cleanup += (sender, e) =>
            {
                disposable.Dispose(); // finish
            };

            return connectable.AsObservable();
        }

        public static IObservable<object> DumpWithColor(this IObservable<EventWrittenEventArgs> source, bool withProviderName = false, Func<EventWrittenEventArgs, string> messageSelector = null)
        {
            return LINQPad.Extensions.Dump(WithColor(source, withProviderName, messageSelector));
        }
    }
}