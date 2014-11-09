using Microsoft.Diagnostics.Tracing;

namespace EtwStream
{
    [EventSource(Name = "EtwStream.EventSource")]
    internal sealed class EtwStreamEventSource : EventSource
    {
        public readonly static EtwStreamEventSource Log = new EtwStreamEventSource();

        private EtwStreamEventSource() { }

        public class Keywords
        {
        }

        public class Tasks
        {
            public const EventTask TraceEventSession = (EventTask)1;
        }

        [Event(1, Task = Tasks.TraceEventSession, Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void SessionBegin(string sessionName)
        {
            WriteEvent(1, sessionName ?? "");
        }

        [Event(2, Task = Tasks.TraceEventSession, Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void SessionEnd(string sessionName)
        {
            WriteEvent(2, sessionName ?? "");
        }

        [Event(3, Level = EventLevel.Verbose)]
        public void CacheGenerate(string eventSourceName)
        {
            WriteEvent(3, eventSourceName ?? "");
        }

        [Event(4, Level = EventLevel.Error)]
        public void CacheGenerateFailed(string message)
        {
            WriteEvent(4, message ?? "");
        }
    }
}