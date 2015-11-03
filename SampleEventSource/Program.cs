using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EtwStream;
using static System.Net.WebRequestMethods;

namespace SampleEventSource
{
    [EventSource(Name = "SampleEventSource")]
    public sealed class SampleEventSource : EventSource
    {
        public static readonly SampleEventSource Log = new SampleEventSource();

        public class Keywords
        {
            public const EventKeywords Test = (EventKeywords)1;
        }

        [Event(1, Level = EventLevel.LogAlways, Keywords = Keywords.Test)]
        public void LogAlways(string msg)
        {
            WriteEvent(1, msg ?? "");
        }

        [Event(2, Level = EventLevel.Critical, Keywords = Keywords.Test)]
        public void Critical(string msg)
        {
            WriteEvent(2, msg ?? "");
        }

        [Event(3, Level = EventLevel.Error, Keywords = Keywords.Test)]
        public void Error(string msg)
        {
            WriteEvent(3, msg ?? "");
        }

        [Event(4, Level = EventLevel.Warning, Keywords = Keywords.Test)]
        public void Warning(string msg)
        {
            WriteEvent(4, msg ?? "");
        }

        [Event(5, Level = EventLevel.Informational, Keywords = Keywords.Test)]
        public void Informational(string msg)
        {
            WriteEvent(5, msg ?? "");
        }


        [Event(6, Level = EventLevel.Verbose, Keywords = Keywords.Test)]
        public void Verbose(string msg)
        {
            WriteEvent(6, msg ?? "");
        }
    }

    // emulation of standard unstructured logger
    [EventSource(Name = "LoggerEventSource")]
    public class LoggerEventSource : EventSource
    {
        public static readonly LoggerEventSource Log = new LoggerEventSource();

        public class Keywords
        {
            public const EventKeywords Logging = (EventKeywords)1;
        }

        string FormatPath(string filePath)
        {
            if (filePath == null) return "";

            var xs = filePath.Split('\\');
            var len = xs.Length;
            if (len >= 3)
            {
                return xs[len - 3] + "/" + xs[len - 2] + "/" + xs[len - 1];
            }
            else if (len == 2)
            {
                return xs[len - 2] + "/" + xs[len - 1];
            }
            else if (len == 1)
            {
                return xs[len - 1];
            }
            else
            {
                return "";
            }
        }

        [Event(1, Level = EventLevel.LogAlways, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void LogAlways(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(1, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(2, Level = EventLevel.Critical, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Critical(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(2, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(3, Level = EventLevel.Error, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(3, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(4, Level = EventLevel.Warning, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(4, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(5, Level = EventLevel.Informational, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Informational(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(5, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(6, Level = EventLevel.Verbose, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Verbose(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(6, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(7, Level = EventLevel.Error, Keywords = Keywords.Logging, Version = 1)]
        public void Exception(string type, string stackTrace, string message)
        {
            WriteEvent(7, type ?? "", stackTrace ?? "", message ?? "");
        }

        [Conditional("DEBUG")]
        [Event(8, Level = EventLevel.Verbose, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            WriteEvent(8, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [NonEvent]
        public IDisposable MeasureExecution(string label, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
        {
            return new StopwatchMonitor(this, label ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
        }

        [Event(9, Level = EventLevel.Informational, Keywords = Keywords.Logging, Message = "[{0}][{2}:{3}][{1}]{4}ms")]
        void MeasureExecution(string label, string memberName, string filePath, int line, double duration)
        {
            WriteEvent(9, label ?? "", memberName ?? "", FormatPath(filePath) ?? "", line, duration);
        }

        class StopwatchMonitor : IDisposable
        {
            readonly LoggerEventSource logger;
            readonly string label;
            readonly string memberName;
            readonly string filePath;
            readonly int line;
            Stopwatch stopwatch;

            public StopwatchMonitor(LoggerEventSource logger, string label, string memberName, string filePath, int line)
            {
                this.logger = logger;
                this.label = label;
                this.memberName = memberName;
                this.filePath = filePath;
                this.line = line;
                stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    logger.MeasureExecution(label, memberName, filePath, line, stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch = null;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ObservableEventListener.FromKernelTraceEvent(Microsoft.Diagnostics.Tracing.Parsers.KernelTraceEventParser.Keywords.Default)
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });


            var providingIndex = 0;
            var providingMessages = new[]
            {
                "1:Hello EtwStream",
                "2:Now LINQPad is ETW viewer",
                "3:Logs are Event Stream",
                "4:Everything can compose by Rx",
                "5:EventSource is...",
                "6:Best practice for logging on .NET",
                "1:Hello EtwStream",
                "2:Now LINQPad is ETW viewer",
                "3:Logs are Event Stream",
                "4:Everything can compose by Rx",
                "5:EventSource is...",
                "6:Best practice for logging on .NET",
            };

            Console.WriteLine("1~6:msg");
            while (true)
            {
                var measure = LoggerEventSource.Log.MeasureExecution("Readtime");

                // var msg = Console.ReadLine();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                var msg = providingMessages[providingIndex++];

                var split = msg.Split(':');
                var id = int.Parse(split[0]);
                var message = (split.Length == 2 && split[1] != "")
                    ? split[1]
                    : null;

                measure.Dispose();
                switch (id)
                {
                    case 1:
                        SampleEventSource.Log.LogAlways(message ?? "LogAlways");
                        break;
                    case 2:
                        SampleEventSource.Log.Critical(message ?? "Critical");
                        break;
                    case 3:
                        SampleEventSource.Log.Error(message ?? "Error");
                        break;
                    case 4:
                        SampleEventSource.Log.Warning(message ?? "Warning");
                        break;
                    case 5:
                        SampleEventSource.Log.Informational(message ?? "Informational");
                        break;
                    case 6:
                        SampleEventSource.Log.Verbose(message ?? "Verbose");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}