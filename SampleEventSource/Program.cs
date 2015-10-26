using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive.Linq;
using EtwStream;

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

        /*
            public enum EventLevel
            {
                LogAlways = 0,
                Critical = 1,
                Error = 2,
                Warning = 3,
                Informational = 4,
                Verbose = 5
            }
        */

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

    class Program
    {
        static void Main(string[] args)
        {
            // self subscribe

            var d = ObservableEventListener.FromEventSource("SampleEventSource")
                .Select(x =>
                {
                    Console.ForegroundColor = x.GetColorMap(false).Value;
                    return x.DumpPayloadOrMessage();
                })
                .Subscribe(x =>
                {
                    Console.WriteLine(x);
                });


            // output source

            Console.WriteLine("1~6:msg");
            while (true)
            {
                var msg = Console.ReadLine();
                var split = msg.Split(':');
                var id = int.Parse(split[0]);
                var message = (split.Length == 2 && split[1] != "")
                    ? split[1]
                    : null;
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