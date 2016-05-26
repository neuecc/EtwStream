using EtwStream.Json;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;

namespace EtwStream
{
    /// <summary>
    /// Abstraction of EventWrittenEventArgs and TraceEvent.
    /// </summary>
    public class EventEntry
    {
        public Guid ActivityID { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }

        public EventKeywords Keywords { get; set; }
        public string KeywordName { get; set; }

        public EventLevel Level { get; set; }

        public string FormattedMessage { get; set; }

        public EventOpcode Opcode { get; set; }
        public string OpcodeName { get; set; }

        public EventTask Task { get; set; }
        public string TaskName { get; set; }

        public int Version { get; set; }

        public ReadOnlyCollection<object> Payload { get; set; }
        public ReadOnlyCollection<string> PayloadNames { get; set; }

        public static EventEntry FromTraceEvent(TraceEvent traceEvent)
        {
            return new EventEntry
            {
                ActivityID = traceEvent.ActivityID,
                EventId = (int)traceEvent.ID,
                EventName = traceEvent.EventName,
                Keywords = (EventKeywords)traceEvent.Keywords,
                KeywordName = traceEvent.GetKeywordName(),
                Level = (EventLevel)traceEvent.Level,
                FormattedMessage = traceEvent.FormattedMessage,
                Opcode = (EventOpcode)traceEvent.Opcode,
                OpcodeName = traceEvent.OpcodeName,
                Task = (EventTask)traceEvent.Task,
                TaskName = traceEvent.TaskName,
                Version = traceEvent.Version,
                // TODO:
                //Payload = traceEvent.payload
                // PayloadNames
            };
        }

        public static EventEntry FromEventWrittenEventArgs(EventWrittenEventArgs args)
        {
            return new EventEntry
            {
                ActivityID = args.ActivityId,
                EventId = (int)args.EventId,
                EventName = args.EventName,
                Keywords = (EventKeywords)args.Keywords,
                KeywordName = args.GetKeywordName(),
                Level = (EventLevel)args.Level,
                FormattedMessage = args.DumpFormattedMessage(),
                Opcode = (EventOpcode)args.Opcode,
                // TODO:GetOpcodeName
                // OpcodeName = args.GetKeywordName(),
                Task = (EventTask)args.Task,
                TaskName = args.GetTaskName(),
                Version = args.Version,
                Payload = args.Payload,
                PayloadNames = args.PayloadNames
            };
        }

        // TODO:ToJson, FromJson

        //public string ToJson()
        //{
        //    var names = PayloadNames;
        //    var count = names.Length;


        //    using (var sw = new StringWriter())
        //    using (var jw = new Json.TinyJsonWriter(sw))
        //    {
        //        jw.WriteStartObject();

        //        //jw.WritePropertyName(nameof(ActivityID), 



        //        //for (int i = 0; i < count; i++)
        //        //{
        //        //    var name = names[i];
        //        //    var value = traceEvent.PayloadString(i);

        //        //    jw.WritePropertyName(name);
        //        //    jw.WriteValue(value);
        //        //}


        //        jw.WriteEndObject();
        //        sw.Flush();
        //        return sw.ToString();
        //    }
        //}

        // FromJson

    }
}