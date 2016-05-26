using Microsoft.Diagnostics.Tracing;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public static class RemoteSink
    {
        public static IDisposable LogToRemote(this IObservable<TraceEvent> source, ConnectionMultiplexer connection, RedisChannel channel)
        {
            var sink = new TraceEventSink(connection, channel);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        class TraceEventSink : SinkBase<TraceEvent>
        {
            readonly ISubscriber subscriber;
            readonly RedisChannel channel;
            Task<long> lastPublishMessage;

            public TraceEventSink(ConnectionMultiplexer connection, RedisChannel channel)
            {
                connection.PreserveAsyncOrder = false;
                this.subscriber = connection.GetSubscriber();
                this.channel = channel;
            }

            public override void OnNext(TraceEvent value)
            {
                // note:serialize event.

                // EventWrittenEventArgs a;
                
                //this.subscriber.PublishAsync(

                base.OnNext(value);
            }

            public override void OnNext(IList<TraceEvent> value)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                if (lastPublishMessage != null)
                {
                    lastPublishMessage.Wait();
                }
            }
        }
    }
}