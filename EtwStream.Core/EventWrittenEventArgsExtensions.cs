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
        // { EventSource : { EventId, EventSchemaPortion } }
        readonly static ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, EventSchemaPortion>> cache = new ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, EventSchemaPortion>>();

        static ReadOnlyDictionary<int, EventSchemaPortion> GetEventSchemaPortions(System.Diagnostics.Tracing.EventSource source)
        {
            return cache.GetOrAdd(source, s => // no needs lock
            {
                var manifest = System.Diagnostics.Tracing.EventSource.GenerateManifest(s.GetType(), null);

                var xElem = XElement.Parse(manifest);
                var ns = xElem.Name.Namespace;

                // { tid : { eventId, keywords, taskName} }
                var tidRef = xElem.Descendants(ns + "event")
                    .ToDictionary(x => x.Attribute("template").Value, x => new
                    {
                        EventId = int.Parse(x.Attribute("value").Value),
                        Keywords = x.Attribute("keywords")?.Value ?? "",
                        Task = x.Attribute("task").Value
                    });


                var dict = xElem.Descendants(ns + "template")
                     .Select(template => new { template, @event = tidRef[template.Attribute("tid").Value] })
                     .ToDictionary(
                        x => x.@event.EventId,
                        x => new EventSchemaPortion(
                                new ReadOnlyCollection<string>(x.template.Elements(ns + "data")
                                .Select(y => y.Attribute("name").Value)
                                .ToArray()),
                                x.@event.Keywords,
                                x.@event.Task));

                return new ReadOnlyDictionary<int, EventSchemaPortion>(dict);
            });
        }

        /// <summary>
        /// Get PeyloadNames from EventSource manifest.
        /// </summary>
        public static ReadOnlyCollection<string> GetPayloadNames(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = GetEventSchemaPortions(source);
            return templates[eventArgs.EventId].Payload;
        }

        /// <summary>
        /// Get KeywordDescription from EventSource manifest.
        /// </summary>
        public static string GetKeywordDescription(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = GetEventSchemaPortions(source);
            return templates[eventArgs.EventId].KeywordDesciption;
        }

        /// <summary>
        /// Get TaskName from EventSource manifest.
        /// </summary>
        public static string GetTaskName(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = GetEventSchemaPortions(source);
            return templates[eventArgs.EventId].TaskName;
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

    /// <summary>
    /// A portion of EventSchema to be required.
    /// </summary>
    class EventSchemaPortion
    {
        internal ReadOnlyCollection<string> Payload { get; }
        internal string KeywordDesciption { get; }
        internal string TaskName { get; }
        internal EventSchemaPortion(ReadOnlyCollection<string> payload, string keywordDescription, string taskName)
        {
            Payload = payload;
            KeywordDesciption = keywordDescription;
            TaskName = taskName;
        }
    }
}