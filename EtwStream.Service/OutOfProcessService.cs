using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwStream.Service
{
    public class OutOfProcessService
    {
        int timeoutMilliseconds = 3000;
        CancellationTokenSource source;
        TaskContainer taskContainer;

        public OutOfProcessService()
        {
            this.source = new CancellationTokenSource();
        }

        public void Start()
        {
            try
            {
                var evaluator = new ScriptingEvaluator();
                var t = Interlocked.Exchange(ref taskContainer, null);
                if (t != null)
                {
                    t.WaitComplete(timeoutMilliseconds);
                }
                taskContainer = evaluator.EvaluateAsync(source.Token).Result;
            }
            catch (Exception ex)
            {
                // TODO:what to do?
                Console.WriteLine(ex);
            }
        }

        public void Stop()
        {
            source.Cancel(); // send terminate event
            var t = Interlocked.Exchange(ref taskContainer, null);
            if (t != null)
            {
                t.WaitComplete(timeoutMilliseconds);
            }
        }
    }
}