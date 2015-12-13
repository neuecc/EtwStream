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
        readonly object evaluatorLock = new object();
        ScriptingEvaluator evaluator = null;

        public void Start()
        {
            try
            {
                lock (evaluatorLock)
                {
                    if (evaluator != null)
                    {
                        evaluator.CancellationTokenSource.Cancel();     // publish OnCompleted
                        (evaluator.EtwStreamService.Container as SubscriptionContainer)?.Dispose(); // wait all subscriptions
                    }

                    evaluator = new ScriptingEvaluator();
                }

                evaluator.EvaluateAsync().Wait();
            }
            catch (Exception ex)
            {
                EtwStreamEventSource.Log.ServiceError("csx evaluator error.", ex.ToString());
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                Console.WriteLine("go stop");
                lock (evaluatorLock)
                {
                    evaluator.CancellationTokenSource.Cancel();
                    (evaluator.EtwStreamService.Container as SubscriptionContainer)?.Dispose(); // wait all subscriptions
                    evaluator = null;
                }
                Console.WriteLine("out stop");
            }
            catch (Exception ex)
            {
                EtwStreamEventSource.Log.ServiceError("evaluator terminate error.", ex.ToString());
                throw;
            }
        }
    }
}