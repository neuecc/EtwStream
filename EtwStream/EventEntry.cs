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
        static readonly ReadOnlyCollection<object> EmptyPayload = new ReadOnlyCollection<object>(new object[0]);
        static readonly ReadOnlyCollection<string> EmptyPayloadNames = new ReadOnlyCollection<string>(new string[0]);
        static readonly Encoding Encoding = new UTF8Encoding(false);

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

        public byte[] Serialize()
        {
            // [16] ActivityID
            // [4] EventId
            // [4] Length of EventName
            // [x] EventName
            // [8] Keywords
            // [4] Length of KeywordName
            // [x] KeywordName
            // [4] Level
            // [4] Length of FormattedMessage
            // [x] FormattedMessage
            // [4] Opcode
            // [4] Length of OpcodeName
            // [x] OpcodeName
            // [4] Task
            // [4] Length of TaskName
            // [x] TaskName
            // [4] Version

            // [4] Payload Length
            // -- Loop: [4] PayloadType, [,]Length + Value
            // [4] PayloadNames Length
            // -- Loop: [4] NameLength, [x] Name


            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms, new UTF8Encoding(false)))
            {
                bw.Write(this.ActivityID.ToByteArray());
                bw.Write(this.EventId);
                bw.Write(this.EventName?.Length ?? 0);// todo:encode!
                bw.Write(this.EventName ?? "");

                bw.Write((long)this.Keywords);
                bw.Write(this.KeywordName?.Length ?? 0);// todo:encode!
                bw.Write(this.KeywordName ?? "");
                bw.Write((int)this.Level);
                bw.Write(this.FormattedMessage?.Length ?? 0); // todo:encode!
                bw.Write(this.FormattedMessage ?? "");
                bw.Write((int)this.Opcode);
                bw.Write(this.OpcodeName?.Length ?? 0);// todo:encode!
                bw.Write(this.OpcodeName ?? "");
                bw.Write((int)this.Task);
                bw.Write(this.TaskName ?? "");
                bw.Write(this.Version);

                var payload = this.Payload ?? EmptyPayload;
                bw.Write(payload.Count);
                foreach (var item in payload)
                {
                    WritePayload(bw, item);
                }

                var payloadNames = this.PayloadNames ?? EmptyPayloadNames;
                bw.Write(payloadNames.Count);
                foreach (var item in payloadNames)
                {
                    var b = Encoding.GetBytes(item);
                    bw.Write(b.Length);
                    bw.Write(b);
                }



                return ms.ToArray();
            }
        }

        void WritePayload(BinaryWriter bw, object value)
        {
            // TODO:
        }


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