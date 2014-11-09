using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EtwStream
{
    public class EventSchema
    {
        public string PayloadName { get; set; }
        public string EventName { get; private set; }
        public string OpcodeName { get; private set; }
        public string TaskName { get; private set; }

        /// <summary>
        ///  <pre>A name for the event. This is simply the concatenation of the task and opcode names (separated by a /). If the event has no opcode, then the event name is just the task name.</pre>
        ///  <pre>Compatible with TraceEvent.</pre>
        /// </summary>
        public string TraceEventEventName
        {
            // TODO:move to EventWrittenEventArgs Extension
            get
            {
                if (OpcodeName == null)
                {
                    return TaskName;
                }
                else
                {
                    return TaskName + "/" + OpcodeName;
                }
            }
        }

        public EventSchema(string payloadName, string eventName, string opcodeName, string taskName)
        {
            this.PayloadName = payloadName;
            this.EventName = eventName;
            this.OpcodeName = opcodeName;
            this.TaskName = taskName;
        }
    }

    public static class MicrosoftEventWrittenEventArgsExtensions
    {
        // { EventSource : { EventId, PayloadNames[] } }
        readonly static ConcurrentDictionary<Microsoft.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>> cache = new ConcurrentDictionary<Microsoft.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>>();

        public static ReadOnlyCollection<string> GetPayloadNames(this Microsoft.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = cache.GetOrAdd(source, s => // no needs lock
            {
                EtwStreamEventSource.Log.CacheGenerate(s.Name);
                try
                {
                    var manifest = Microsoft.Diagnostics.Tracing.EventSource.GenerateManifest(s.GetType(), null);

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
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.CacheGenerateFailed(ex.ToString());
                    throw;
                }
            });

            return templates[eventArgs.EventId];
        }

        public static string DumpPayload(this Microsoft.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.GetPayloadNames();

            var sb = new StringBuilder();
            // TODO:fix same as TraceEvent!
            sb.Append(eventArgs.EventName).Append(": ");
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

        public static string DumpPayloadOrMessage(this Microsoft.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var msg = eventArgs.Message;
            return string.IsNullOrWhiteSpace(msg) ? eventArgs.DumpPayload() : msg;
        }

        public static IEnumerable<KeyValuePair<string, object>> MergePayloadValues(this Microsoft.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.GetPayloadNames();
            var values = eventArgs.Payload;

            var count = names.Count;
            for (int i = 0; i < count; i++)
            {
                yield return new KeyValuePair<string, object>(names[i], values[i]);
            }
        }
    }

    public static class SystemEventWrittenEventArgsExtensions
    {
        // { EventSource : { EventId, PayloadNames[] } }
        readonly static ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>> cache = new ConcurrentDictionary<System.Diagnostics.Tracing.EventSource, ReadOnlyDictionary<int, ReadOnlyCollection<string>>>();

        public static ReadOnlyCollection<string> GetPayloadNames(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var source = eventArgs.EventSource;
            var templates = cache.GetOrAdd(source, s => // no needs lock
            {
                EtwStreamEventSource.Log.CacheGenerate(s.Name);
                try
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
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.CacheGenerateFailed(ex.ToString());
                    throw;
                }
            });

            return templates[eventArgs.EventId];
        }

        public static string DumpPayload(this System.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.GetPayloadNames();

            var sb = new StringBuilder();
            // TODO:needs EventName
            // sb.Append(eventArgs.EventName).Append(": ");
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
            return string.IsNullOrWhiteSpace(msg) ? eventArgs.DumpPayload() : msg;
        }

        public static IEnumerable<KeyValuePair<string, object>> MergePayloadValues(this Microsoft.Diagnostics.Tracing.EventWrittenEventArgs eventArgs)
        {
            var names = eventArgs.GetPayloadNames();
            var values = eventArgs.Payload;

            var count = names.Count;
            for (int i = 0; i < count; i++)
            {
                yield return new KeyValuePair<string, object>(names[i], values[i]);
            }
        }
    }
}
