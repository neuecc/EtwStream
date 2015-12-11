using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;

namespace EtwStream.Sinks
{
    public static class FileSink
    {

        // Sinks

        class TraceEventSink : SinkBase<TraceEvent>
        {
            StreamWriter streamWriter;

            public TraceEventSink(bool isAsync = true)
            {

            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override void OnNext(IList<TraceEvent> value)
            {
                foreach (var item in value)
                {
                    streamWriter.WriteLine(item);
                }
            }
        }
    }
}
