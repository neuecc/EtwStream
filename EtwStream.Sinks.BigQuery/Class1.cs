using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigQuery.Linq;
using Google.Apis.Util;
using Newtonsoft.Json;

namespace EtwStream
{
    public static class BigQuerySink
    {


        class MyClass<T> : SinkBase<T>
        {
            public MyClass(
                Google.Apis.Bigquery.v2.BigqueryService bigqueryService,
                string projectId,
                Func<T, string> datasetIdSelector,
                Func<T, string> tableIdSelector,
                IBackOff retryStrategy = null, 
                Func<T, string> insertIdSelector = null, 
                JsonSerializerSettings serializerSettings = null)
            {
                
                new MetaTable("pid", "dtid", "tid").InsertAllAsync(bigqueryService, 
            }

            public override void OnNext(IList<Task> value)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
