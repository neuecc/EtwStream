using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public static class TraceEventExtensions
    {
        public static string DumpPayload(this TraceEvent traceEvent)
        {
            var names = traceEvent.PayloadNames;

            var sb = new StringBuilder();
            sb.Append(traceEvent.EventName).Append(": ");
            sb.Append("{");
            var count = names.Length;
            for (int i = 0; i < count; i++)
            {
                if (i != 0) sb.Append(", ");
                var name = names[i];
                var value = traceEvent.PayloadValue(i);
                sb.Append(name).Append(": ").Append(value);
            }
            sb.Append("}");

            return sb.ToString();
        }

        public static string DumpPayloadOrMessage(this TraceEvent traceEvent)
        {
            var msg = traceEvent.FormattedMessage;
            return string.IsNullOrWhiteSpace(msg) ? traceEvent.DumpPayload() : msg;
        }

        public static IEnumerable<KeyValuePair<string, object>> MergePayloadValues(this TraceEvent traceEvent)
        {
            var names = traceEvent.PayloadNames;

            for (int i = 0; i < names.Length; i++)
            {
                yield return new KeyValuePair<string, object>(names[i], traceEvent.PayloadValue(i));
            }
        }
    }
}