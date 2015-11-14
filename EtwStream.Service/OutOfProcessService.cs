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
        ScriptingCompletion completion;

        public OutOfProcessService()
        {
            this.source = new CancellationTokenSource();
        }

        public async void Start()
        {
            try
            {
                var evaluator = new ScriptingEvaluator();
                var t = Interlocked.Exchange(ref completion, null);
                if (t != null)
                {
                    t.WaitComplete(timeoutMilliseconds);
                }
                completion = await evaluator.EvaluateAsync(source.Token);
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
            var t = Interlocked.Exchange(ref completion, null);
            if (t != null)
            {
                t.WaitComplete(timeoutMilliseconds);
            }
        }
    }
}