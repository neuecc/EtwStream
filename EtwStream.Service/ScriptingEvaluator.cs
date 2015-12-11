using System;
using System.Collections.Generic;
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
        string LoadScript(string fileName)
        {
            var code = File.ReadAllText("configuration.csx");
            code = code.TrimEnd(' ', ';', '\r', '\n'); // needs the trim, last line must be expression
            return code;
        }

        public async Task<TaskContainer> EvaluateAsync(CancellationToken terminateToken)
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
                .WithImports(
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

            await CSharpScript.EvaluateAsync(code, options, globalParameter, typeof(Globals), terminateToken).ConfigureAwait(false);

            return globalParameter.EtwStreamService.TaskContainer;
        }
    }

    public class EtwStreamService
    {
        public CancellationToken TerminateToken { get; }
        public TaskContainer TaskContainer { get; }
        public IReadOnlyDictionary<string, string> AppConfig { get; }

        public EtwStreamService(CancellationToken terminateToken, IReadOnlyDictionary<string, string> appConfig)
        {
            this.TerminateToken = terminateToken;
            this.AppConfig = appConfig;
            this.TaskContainer = new TaskContainer();
        }
    }

    public class TaskContainer
    {
        List<Task> list = new List<Task>();

        public void Add(Task task)
        {
            lock (list)
            {
                list.Add(task);
            }
        }

        internal void WaitComplete(int millisecondsTimeout)
        {
            Task[] array;
            lock (list)
            {
                array = list.ToArray();
            }
            Task.WaitAll(array, millisecondsTimeout);
        }
    }
}

namespace EtwStream
{
    public static class TaskEtwStreamServiceExtensions
    {
        public static void AddTo(this Task task, EtwStream.Service.TaskContainer container)
        {
            container.Add(task);
        }
    }
}