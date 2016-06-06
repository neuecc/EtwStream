using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace EtwStream
{
    public static class TraceEventExtensions
    {
        //{ providerGuid : { eventId: KeywordName }}
        readonly static ConcurrentDictionary<Guid, ReadOnlyDictionary<int, string>> cache = new ConcurrentDictionary<Guid, ReadOnlyDictionary<int, string>>();

        internal static void CacheSchema(ProviderManifest manifest)
        {
            var xElem = XElement.Parse(manifest.Manifest);
            var ns = xElem.DescendantsAndSelf().First(x => x.Name.LocalName != "Event").Name.Namespace;

            // { tid : {[payloadNames]}}
            var tidRef = xElem.Descendants(ns + "template")
                .ToDictionary(x => x.Attribute("tid").Value, x => new ReadOnlyCollection<string>(
                    x.Elements(ns + "data")
                    .Select(y => y.Attribute("name").Value)
                    .ToArray()));

            var dict = xElem.Descendants(ns + "event")
                .ToDictionary(
                    x => int.Parse(x.Attribute("value").Value),
                    x => x.Attribute("keywords")?.Value ?? "");

            var readOnlyDict = new ReadOnlyDictionary<int, string>(dict);
            cache[manifest.Guid] = readOnlyDict;
        }

        public static string GetKeywordName(this TraceEvent traceEvent)
        {
            ReadOnlyDictionary<int, string> schema;
            string name;
            return cache.TryGetValue(traceEvent.ProviderGuid, out schema)
                ? schema.TryGetValue((int)traceEvent.ID, out name)
                    ? name
                    : traceEvent.Keywords.ToString()
                : traceEvent.Keywords.ToString();
        }

        public static ConsoleColor? GetColorMap(this TraceEvent traceEvent, bool isBackgroundWhite)
        {
            switch (traceEvent.Level)
            {
                case TraceEventLevel.Critical:
                    return ConsoleColor.Magenta;
                case TraceEventLevel.Error:
                    return ConsoleColor.Red;
                case TraceEventLevel.Informational:
                    return ConsoleColor.Green;
                case TraceEventLevel.Verbose:
                    return ConsoleColor.Gray;
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
                var value = traceEvent.PayloadString(i);
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
                    var value = traceEvent.PayloadString(i);

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