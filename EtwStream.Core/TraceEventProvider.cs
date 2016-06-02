using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace EtwStream
{
    public class TraceEventProvider
    {
        public Guid Guid;
        public TraceEventLevel Level;

        public TraceEventProvider(string nameOrGuid, TraceEventLevel level = TraceEventLevel.Verbose)
        {
            if (!Guid.TryParse(nameOrGuid, out Guid))
            {
                Guid = TraceEventProviders.GetEventSourceGuidFromName(nameOrGuid);
            }
            Level = level;
        }

        public TraceEventProvider(Guid guid, TraceEventLevel level = TraceEventLevel.Verbose)
        {
            Guid = guid;
            Level = level;
        }
    }
}