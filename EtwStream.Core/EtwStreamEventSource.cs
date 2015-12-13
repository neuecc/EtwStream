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

        EtwStreamEventSource()
        {

        }

        [Event(1, Level = EventLevel.Error)]
        public void SinkError(string sinkName, string message, string error)
        {
            WriteEvent(1, sinkName ?? "", message ?? "", error ?? "");
        }
    }
}
