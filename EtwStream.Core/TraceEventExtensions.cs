using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace EtwStream
{
    public static class TraceEventExtensions
    {
        public static ConsoleColor? GetColorMap(this TraceEvent traceEvent, bool isBackgroundWhite)
        {
            switch (traceEvent.Level)
            {
                case TraceEventLevel.Critical:
                    return ConsoleColor.Magenta;
                case TraceEventLevel.Error:
                    return ConsoleColor.Red;
                case TraceEventLevel.Informational:
                    return ConsoleColor.Gray;
                case TraceEventLevel.Verbose:
                    return ConsoleColor.Green;
                case TraceEventLevel.Warning:
                    return isBackgroundWhite ? ConsoleColor.DarkRed : ConsoleColor.Yellow;
                case TraceEventLevel.Always:
                    return isBackgroundWhite ? ConsoleColor.Black : ConsoleColor.White;
                default:
                    return null;
            }
        }

        public static string DumpPayload(this TraceEvent traceEvent)
        {
            var names = traceEvent.PayloadNames;

            var sb = new StringBuilder();
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

        public static string ToJson(this TraceEvent traceEvent)
        {
            var names = traceEvent.PayloadNames;
            var count = names.Length;

            using (var sw = new StringWriter())
            using (var jw = new Json.TinyJsonWriter(sw))
            {
                jw.WriteStartObject();
                for (int i = 0; i < count; i++)
                {
                    var name = names[i];
                    var value = traceEvent.PayloadValue(i);

                    jw.WritePropertyName(name);
                    jw.WriteValue(value);
                }
                jw.WriteEndObject();
                sw.Flush();
                return sw.ToString();
            }
        }
    }
}