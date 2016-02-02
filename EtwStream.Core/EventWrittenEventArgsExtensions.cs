using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EtwStream
{
    public static class EventWrittenEventArgsExtensions
    {
        public static string DumpFormattedMessage(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var msg = eventArgs.Message;
            if (string.IsNullOrWhiteSpace(msg)) return msg;

            return string.Format(msg, eventArgs.Payload.ToArray());
        }

        public static string DumpPayload(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.PayloadNames;

            var sb = new StringBuilder();
            sb.Append("{");
            var count = eventArgs.Payload.Count;
            for (int i = 0; i < count; i++)
            {
                if (i != 0) sb.Append(", ");
                var name = names[i];
                var value = eventArgs.Payload[i];
                sb.Append(name).Append(": ").Append(value);
            }
            sb.Append("}");

            return sb.ToString();
        }

        public static string DumpPayloadOrMessage(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var msg = eventArgs.Message;
            return string.IsNullOrWhiteSpace(msg) ? eventArgs.DumpPayload() : DumpFormattedMessage(eventArgs);
        }

        public static ConsoleColor? GetColorMap(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs, bool isBackgroundWhite)
        {
            switch (eventArgs.Level)
            {
                case EventLevel.Critical:
                    return ConsoleColor.Magenta;
                case EventLevel.Error:
                    return ConsoleColor.Red;
                case EventLevel.Informational:
                    return ConsoleColor.Green;
                case EventLevel.Verbose:
                    return ConsoleColor.Gray;
                case EventLevel.Warning:
                    return isBackgroundWhite ? ConsoleColor.DarkRed : ConsoleColor.Yellow;
                case EventLevel.LogAlways:
                    return isBackgroundWhite ? ConsoleColor.Black : ConsoleColor.White;
                default:
                    return null;
            }
        }

        public static string ToJson(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.PayloadNames;
            var count = names.Count;

            using (var sw = new StringWriter())
            using (var jw = new Json.TinyJsonWriter(sw))
            {
                jw.WriteStartObject();
                for (int i = 0; i < count; i++)
                {
                    var name = names[i];
                    var value = eventArgs.Payload[i];

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