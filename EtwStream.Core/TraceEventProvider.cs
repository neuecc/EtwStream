using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace EtwStream
{
    public class TraceEventProvider
    {
        public Guid Guid { get; }
        public TraceEventLevel Level { get; }

        public TraceEventProvider(string nameOrGuid, TraceEventLevel level = TraceEventLevel.Verbose)
        {
            Guid guid;
            if (!Guid.TryParse(nameOrGuid, out guid))
            {
                this.Guid = TraceEventProviders.GetEventSourceGuidFromName(nameOrGuid);
            }
            else
            {
                this.Guid = guid;
            }
            this.Level = level;
        }

        public TraceEventProvider(Guid guid, TraceEventLevel level = TraceEventLevel.Verbose)
        {
            this.Guid = guid;
            this.Level = level;
        }
    }
}