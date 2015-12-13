using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    [EventSource(Name = "EtwStreamEventSource")]
    public class EtwStreamEventSource : EventSource
    {
        public static readonly EtwStreamEventSource Log = new EtwStreamEventSource();

        public class Keywords
        {
            public const EventKeywords Logger = (EventKeywords)1;
            public const EventKeywords Sink = (EventKeywords)2;
            public const EventKeywords Service = (EventKeywords)4;
        }

        EtwStreamEventSource()
        {

        }

        [Event(1, Level = EventLevel.LogAlways, Keywords = Keywords.Logger)]
        public void LogAlways(string msg)
        {
            WriteEvent(1, msg ?? "");
        }

        [Event(2, Level = EventLevel.Critical, Keywords = Keywords.Logger)]
        public void Critical(string msg)
        {
            WriteEvent(2, msg ?? "");
        }

        [Event(3, Level = EventLevel.Error, Keywords = Keywords.Logger)]
        public void Error(string msg)
        {
            WriteEvent(3, msg ?? "");
        }

        [Event(4, Level = EventLevel.Warning, Keywords = Keywords.Logger)]
        public void Warning(string msg)
        {
            WriteEvent(4, msg ?? "");
        }

        [Event(5, Level = EventLevel.Informational, Keywords = Keywords.Logger)]
        public void Informational(string msg)
        {
            WriteEvent(5, msg ?? "");
        }

        [Event(6, Level = EventLevel.Verbose, Keywords = Keywords.Logger)]
        public void Verbose(string msg)
        {
            WriteEvent(6, msg ?? "");
        }

        [Event(7, Level = EventLevel.Error, Keywords = Keywords.Sink)]
        public void SinkError(string sinkName, string message, string error)
        {
            WriteEvent(7, sinkName ?? "", message ?? "", error ?? "");
        }

        [Event(8, Level = EventLevel.Error, Keywords = Keywords.Service)]
        public void ServiceError(string message, string error)
        {
            WriteEvent(8, message ?? "", error ?? "");
        }
    }
}
