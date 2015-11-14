using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EtwStream
{
    public static class EventWrittenEventArgsExtensions
    {
        // { EventSource : { EventId, PayloadNames[] } }
        readonly static ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>> cache = new ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>>();

        public static ReadOnlyCollection<string> GetPayloadNames(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = cache.GetOrAdd(source, s => // no needs lock
            {
                var manifest = System.Diagnostics.Tracing.EventSource.GenerateManifest(s.GetType(), null);

                var xElem = XElement.Parse(manifest);
                var ns = xElem.Name.Namespace;

                // { tid : eventId }
                var tidRef = xElem.Descendants(ns + "event")
                    .ToDictionary(x => x.Attribute("template").Value, x => x.Attribute("value").Value);

                var dict = xElem.Descendants(ns + "template")
                     .ToDictionary(
                        x => int.Parse(tidRef[x.Attribute("tid").Value]),
                        x => new ReadOnlyCollection<string>(x.Elements(ns + "data")
                            .Select(y => y.Attribute("name").Value)
                            .ToArray()));

                return new ReadOnlyDictionary<int, ReadOnlyCollection<string>>(dict);
            });

            return templates[eventArgs.EventId];
        }

        public static string DumpFormattedMessage(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var msg = eventArgs.Message;
            if (string.IsNullOrWhiteSpace(msg)) return msg;

            return string.Format(msg, eventArgs.Payload.ToArray());
        }

        public static string DumpPayload(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.GetPayloadNames();

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
                    return ConsoleColor.Gray;
                case EventLevel.Verbose:
                    return ConsoleColor.Green;
                case EventLevel.Warning:
                    return isBackgroundWhite ? ConsoleColor.DarkRed : ConsoleColor.Yellow;
                case EventLevel.LogAlways:
                    return isBackgroundWhite ? ConsoleColor.Black : ConsoleColor.White;
                default:
                    return null;
            }
        }
    }
}