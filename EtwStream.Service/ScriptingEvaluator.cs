using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace EtwStream.Service
{
    public class Globals
    {
        public EtwStreamService EtwStreamService;
    }

    public class ScriptingEvaluator
    {
        string LoadScript(string fileName)
        {
            var code = File.ReadAllText("configuration.csx");
            code = code.TrimEnd(' ', ';', '\r', '\n'); // needs the trim, last line must be expression
            return code;
        }

        public async Task<ScriptingCompletion> EvaluateAsync(CancellationToken terminateToken)
        {
            // TODO:read code
            var code = LoadScript("configuration.csx");

            var options = ScriptOptions.Default
                .AddReferences(new[]
                {
                    typeof(System.Exception).Assembly,
                    typeof(System.Linq.Enumerable).Assembly,
                    typeof(System.Reactive.Notification).Assembly,
                    typeof(System.Reactive.Concurrency.IScheduler).Assembly,
                    typeof(System.Reactive.Linq.Observable).Assembly,
                    typeof(ObservableEventListener).Assembly,
                    typeof(Microsoft.Diagnostics.Tracing.TraceEvent).Assembly
                })
                .AddNamespaces(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Threading.Tasks",
                    "System.Reactive.Linq",
                    "System.Reactive.Disposables",
                    "EtwStream");

            var globalParameter = new Globals()
            {
                // TODO:AppConfig
                EtwStreamService = new EtwStreamService(terminateToken, new Dictionary<string, string>())
            };

            var result = await CSharpScript.EvaluateAsync(code, options, globalParameter, typeof(Globals), terminateToken).ConfigureAwait(false);
            var completion = result as ScriptingCompletion;
            if (completion == null)
            {
                // TODO:Exception
                throw new InvalidOperationException("last line must returns EtwStreamService.CompleteConfiguration(all ObservableEventListener's subscriptions)");
            }

            return completion;
        }
    }

    public class EtwStreamService
    {
        public CancellationToken TerminateToken { get; }
        public IReadOnlyDictionary<string, string> AppConfig { get; }

        public ScriptingCompletion CompleteConfiguration(params Task[] asyncSubscriptions)
        {
            return new ScriptingCompletion(asyncSubscriptions);
        }

        public EtwStreamService(CancellationToken terminateToken, IReadOnlyDictionary<string, string> appConfig)
        {
            this.TerminateToken = terminateToken;
            this.AppConfig = appConfig;
        }
    }

    public class ScriptingCompletion
    {
        public Task[] AsyncSubscriptions { get; }

        public ScriptingCompletion(Task[] asyncSubscriptions)
        {
            this.AsyncSubscriptions = asyncSubscriptions;
        }

        public void WaitComplete(int millisecondsTimeout)
        {
            Task.WaitAll(AsyncSubscriptions, millisecondsTimeout);
        }
    }
}