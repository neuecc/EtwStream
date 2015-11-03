using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Session;

namespace EtwStream
{
    public static class WellKnownEventSources
    {
        public static Guid FrameworkEventSource => Guid.Parse("8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1");
        public static Guid ConcurrentCollectionsEventSource => Guid.Parse("35167F8E-49B2-4b96-AB86-435B59336B5E");
        public static Guid SynchronizationEventSource => Guid.Parse("EC631D38-466B-4290-9306-834971BA0217");
        public static Guid TplEventSource => Guid.Parse("2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5");
        public static Guid PinnableBufferCacheEventSource => TraceEventProviders.GetEventSourceGuidFromName("Microsoft-DotNETRuntime-PinnableBufferCache-System");
        public static Guid PlinqEventSource => Guid.Parse("159eeeec-4a14-4418-a8fe-faabcd987887");
        public static Guid SqlEventSource => TraceEventProviders.GetEventSourceGuidFromName("Microsoft-AdoNet-SystemData");
        public static Guid AspNetEventSource => Guid.Parse("ee799f41-cfa5-550b-bf2c-344747c1c668");
    }

    public static class IISEventSources
    {
        public static Guid HttpEvent => Guid.Parse("7B6BC78C-898B-4170-BBF8-1A469EA43FC5");
        public static Guid HttpLog => Guid.Parse("C42A2738-2333-40A5-A32F-6ACC36449DCC");
        public static Guid HttpService => Guid.Parse("DD5EF90A-6398-47A4-AD34-4DCECDEF795F");
        public static Guid RuntimeWebHttp => Guid.Parse("41877CB4-11FC-4188-B590-712C143C881D");
        public static Guid RuntimeWebApi => Guid.Parse("6BD96334-DC49-441A-B9C4-41425BA628D8");
        public static Guid AspDotNetEvents => Guid.Parse("AFF081FE-0247-4275-9C4E-021F3DC1DA35");
        public static Guid IISAppHostSvc => Guid.Parse("CAC10856-9223-48FE-96BA-2A772274FB53");
        public static Guid IISLogging => Guid.Parse("7E8AD27F-B271-4EA2-A783-A47BDE29143B");
        public static Guid IISW3Svc => Guid.Parse("05448E22-93DE-4A7A-BBA5-92E27486A8BE");
    }
}