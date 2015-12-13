using System;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EtwStream.Service
{
    public class Globals
    {
        public EtwStreamService EtwStreamService;
    }

    public class ScriptingEvaluator
    {
        public EtwStreamService EtwStreamService { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public ScriptingEvaluator()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            this.EtwStreamService = new EtwStreamService(this.CancellationTokenSource.Token);
        }

        string LoadScript(string fileName)
        {
            var code = File.ReadAllText("configuration.csx");
            code = code.TrimEnd(' ', ';', '\r', '\n');
            return code;
        }

        public async Task EvaluateAsync()
        {
            var code = LoadScript("configuration.csx");
            
            var options = ScriptOptions.Default
                .AddReferences(new[]
                {
                    this.GetType().Assembly,
                    typeof(System.Exception).Assembly,
                    typeof(System.Linq.Enumerable).Assembly,
                    typeof(System.Configuration.ConfigurationManager).Assembly,
                    typeof(System.Reactive.Notification).Assembly,
                    typeof(System.Reactive.Concurrency.IScheduler).Assembly,
                    typeof(System.Reactive.Linq.Observable).Assembly,
                    typeof(ObservableEventListener).Assembly,
                    typeof(Microsoft.Diagnostics.Tracing.TraceEvent).Assembly
                })
                .WithImports(
                    "System",
                    "System.IO",
                    "System.Diagnostics",
                    "System.Dynamic",
                    "System.Linq",
                    "System.Linq.Expressions",
                    "System.Text",
                    "System.Collections.Generic",
                    "System.Threading.Tasks",
                    "System.Reactive.Linq",
                    "System.Reactive.Disposables",
                    "System.Configuration",
                    "EtwStream");

            var globalParameter = new Globals()
            {
                EtwStreamService = this.EtwStreamService
            };

            await CSharpScript.EvaluateAsync(code, options, globalParameter, typeof(Globals), this.EtwStreamService.TerminateToken).ConfigureAwait(false);
        }
    }

    public class EtwStreamService
    {
        public CancellationToken TerminateToken { get; }
        public ISubscriptionContainer Container { get; }

        public EtwStreamService(CancellationToken terminateToken)
        {
            this.TerminateToken = terminateToken;
            this.Container = new SubscriptionContainer();
        }
    }
}