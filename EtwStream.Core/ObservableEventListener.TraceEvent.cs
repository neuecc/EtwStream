using Microsoft.Diagnostics.Tracing;
using System.Reactive.Linq;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace EtwStream
{
    // Out-of-Process(TraceEvent) Listener

    public static partial class ObservableEventListener
    {
        const string ManifestEventName = "ManifestData";
        const TraceEventID ManifestEventID = (TraceEventID)0xFFFE;

        public static IObservableEventListener<TraceEvent> FromTraceEvent(string providerName)
        {
            return FromTraceEvent(TraceEventProviders.GetEventSourceGuidFromName(providerName));
        }

        /// <summary>
        /// Observer Out-of-Process ETW Realtime session by provider Guid.
        /// </summary>
        public static IObservableEventListener<TraceEvent> FromTraceEvent(Guid providerGuid)
        {
            var subject = new Subject<TraceEvent>();

            var session = new TraceEventSession("MyRealTimeSession", TraceEventSessionOptions.Create);
            var sessionName = session.SessionName;

            try
            {
                session.Source.Dynamic.Observe((pName, eName) => EventFilterResponse.AcceptEvent)
                    .Where(x => x.ProviderGuid == providerGuid && x.EventName != ManifestEventName && x.ID != ManifestEventID)
                    .Subscribe(subject);
                session.EnableProvider(providerGuid);
            }
            catch
            {
                session.Dispose();
                throw;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    EtwStreamEventSource.Log.SessionBegin(sessionName);
                    session.Source.Process();
                }
                finally
                {
                    EtwStreamEventSource.Log.SessionEnd(sessionName);
                    session.Dispose();
                }
            }, TaskCreationOptions.LongRunning);

            return new TraceEventListener<TraceEvent>(subject, session);
        }

        public static IObservableEventListener<TData> FromTraceEvent<TParser, TData>()
            where TParser : TraceEventParser
            where TData : TraceEvent
        {
            var subject = new Subject<TData>();

            var session = new TraceEventSession("MyRealTimeSession");
            try
            {
                var parser = (TraceEventParser)typeof(TParser).GetConstructor(new[] { typeof(TraceEventSource) }).Invoke(new[] { session.Source });
                var guid = (Guid)typeof(TParser).GetField("ProviderGuid", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                session.EnableProvider(guid);
                parser.Observe<TData>().Subscribe(subject);
            }
            catch
            {
                session.Dispose();
                throw;
            }

            Task.Factory.StartNew(() =>
            {
                using (session)
                {
                    session.Source.Process();
                }
            }, TaskCreationOptions.LongRunning);

            return new TraceEventListener<TData>(subject, session);
        }

        public static IObservableEventListener<TraceEvent> FromClrTraceEvent()
        {
            var subject = new Subject<TraceEvent>();

            var session = new TraceEventSession("MyRealTimeSession");
            try
            {
                var guid = Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser.ProviderGuid;
                session.EnableProvider(guid);
                session.Source.Clr.Observe((pName, eName) => EventFilterResponse.AcceptEvent)
                    .Where(x => x.ProviderGuid == guid && x.EventName != ManifestEventName && x.ID != ManifestEventID)
                    .Subscribe(subject);
            }
            catch
            {
                session.Dispose();
                throw;
            }

            Task.Factory.StartNew(state =>
            {
                using (session)
                {
                    session.Source.Process();
                }
            }, TaskCreationOptions.LongRunning);

            return new TraceEventListener<TraceEvent>(subject, session);
        }

        public static IObservableEventListener<TraceEvent> FromKernelTraceEvent(KernelTraceEventParser.Keywords flags, KernelTraceEventParser.Keywords stackCapture = KernelTraceEventParser.Keywords.None)
        {
            var subject = new Subject<TraceEvent>();

            var session = new TraceEventSession("MyRealTimeSession");
            try
            {
                var guid = KernelTraceEventParser.ProviderGuid;
                session.EnableKernelProvider(flags, stackCapture);
                session.Source.Kernel.Observe((pName, eName) => EventFilterResponse.AcceptEvent)
                    .Where(x => x.ProviderGuid == guid && x.EventName != ManifestEventName && x.ID != ManifestEventID)
                    .Subscribe(subject);
            }
            catch
            {
                session.Dispose();
                throw;
            }

            Task.Factory.StartNew(state =>
            {
                using (session)
                {
                    session.Source.Process();
                }
            }, TaskCreationOptions.LongRunning);

            return new TraceEventListener<TraceEvent>(subject, session);
        }

        public static IObservableEventListener<TData> FromKernelTraceEvent<TData>(KernelTraceEventParser.Keywords flags, KernelTraceEventParser.Keywords stackCapture = KernelTraceEventParser.Keywords.None)
            where TData : TraceEvent
        {
            var subject = new Subject<TData>();

            var session = new TraceEventSession("MyRealTimeSession");
            try
            {
                session.EnableKernelProvider(flags, stackCapture);
                session.Source.Kernel.Observe<TData>().Subscribe(subject);
            }
            catch
            {
                session.Dispose();
                throw;
            }

            Task.Factory.StartNew(state =>
            {
                using (session)
                {
                    session.Source.Process();
                }
            }, TaskCreationOptions.LongRunning);

            return new TraceEventListener<TData>(subject, session);
        }

        class TraceEventListener<T> : IObservableEventListener<T>
        {
            readonly Subject<T> subject;
            readonly TraceEventSession session;

            public TraceEventListener(Subject<T> subject, TraceEventSession session)
            {
                this.subject = subject;
                this.session = session;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return subject.Subscribe(observer);
            }

            public void Dispose()
            {
                subject.Dispose();
                session.Dispose();
            }
        }
    }
}
