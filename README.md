EtwStream
---
[ETW(Event Tracing for Windows)](https://msdn.microsoft.com/en-us/library/windows/desktop/bb968803.aspx) and [EventSource](https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventsource.aspx) is important feature for structured logging in .NET. But monitoring log stream is very hard. EtwStream provides LINQPad integartion, you can dump ETW stream simply like log viewer.

```
PM> Install-Package EtwStream.LinqPad
```

![etwstreamgif](https://cloud.githubusercontent.com/assets/46207/10905625/cae5a122-825e-11e5-8def-d53feedb879d.gif)

`ObservableEventListener` + `WithColor` for Expression/`DumpWithColor` for Statement mode, dump event stream with colored and auto scrolling(stop auto scroll, use `Ctrl+Shift+E` shortcut).

LINQPad's default rows limit is 1000 line. You can expand to 10000 rows at `Preferences -> Results -> Maximum rows to display`.

![image](https://cloud.githubusercontent.com/assets/46207/10906322/e61cde42-8263-11e5-9b96-94935415d778.png)

> EtwStream.LinqPad only supports LINQPad 5. LINQPad 4 isn't supported.

ObservableEventListener
---
ObservableEventListener provides five ways for observe log events.

| Method               | Description
| -------------------- | ---------------------------------------------------------
| FromEventSource      | Observe In-Process EventSource events. It's no across ETW.
| FromTraceEvent       | Observe Out-of-Process ETW Realtime session.
| FromClrTraceEvent    | Observe Out-of-Process ETW CLR TraceEvent.
| FromKernelTraceEvent | Observe Out-of-Process ETW Kernel TraceEvent.
| FromFileTail         | Observe String-Line from file like tail -f.

You usually use FromTraceEvent, it can observer own defined EventSource and built-in EventSource such as TplEventSource. `withProcessName: true`, dump with process name.

![etwstreamtpl](https://cloud.githubusercontent.com/assets/46207/10906637/891344b8-8266-11e5-9bc5-0159bd60f048.gif)

`FromFileTail` is not ETW/EventSouce but useful method. It's like tail -f. FromFileTail does not enable auto scrolling automatically. You should enable AutoScrollResults = true manually.

```csharp
Util.AutoScrollResults = true;

ObservableEventListener.FromFileTail(@"C:\log.txt")
.Dump();
```

with Reactive Extensions
---
Everything is IObservable! You can filter, merge, grouping log events by Reactive Extensions. 

```csharp
Observable.Merge(
    ObservableEventListener.FromTraceEvent("LoggerEventSource"),
    ObservableEventListener.FromTraceEvent("MyCompanyEvent"),
    ObservableEventListener.FromTraceEvent("PhotonWire")
)
.DumpWithColor(withProviderName: true);
```

`withProviderName: true`, shows provider name so you can distinguish merged event source.

EtwStream.Core
---
EtwStream's Core Engine can use all .NET apps.

```
PM> Install-Package EtwStream
```

ObservableEventListener is simple wrapper of `EventListener` and `TraceEvent(Microsoft.Diagnostics.Tracing.TraceEvent)`. You can control there easily. 

LoggerEventSource
---
First step for use EventSource. Here is simply legacy-unstructured-logging style logger.

```csharp
[EventSource(Name = "LoggerEventSource")]
public class LoggerEventSource : EventSource
{
    public static readonly LoggerEventSource Log = new LoggerEventSource();

    public class Keywords
    {
        public const EventKeywords Logging = (EventKeywords)1;
    }

    string FormatPath(string filePath)
    {
        if (filePath == null) return "";

        var xs = filePath.Split('\\');
        var len = xs.Length;
        if (len >= 3)
        {
            return xs[len - 3] + "/" + xs[len - 2] + "/" + xs[len - 1];
        }
        else if (len == 2)
        {
            return xs[len - 2] + "/" + xs[len - 1];
        }
        else if (len == 1)
        {
            return xs[len - 1];
        }
        else
        {
            return "";
        }
    }

    [Event(1, Level = EventLevel.LogAlways, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void LogAlways(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(1, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(2, Level = EventLevel.Critical, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Critical(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(2, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(3, Level = EventLevel.Error, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(3, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(4, Level = EventLevel.Warning, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(4, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(5, Level = EventLevel.Informational, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Informational(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(5, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(6, Level = EventLevel.Verbose, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Verbose(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(6, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(7, Level = EventLevel.Error, Keywords = Keywords.Logging, Version = 1)]
    public void Exception(string type, string stackTrace, string message)
    {
        WriteEvent(7, type ?? "", stackTrace ?? "", message ?? "");
    }

    [Conditional("DEBUG")]
    [Event(8, Level = EventLevel.Verbose, Keywords = Keywords.Logging, Message = "[{2}:{3}][{1}]{0}")]
    public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        WriteEvent(8, message ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [NonEvent]
    public IDisposable MeasureExecution(string label, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int line = 0)
    {
        return new StopwatchMonitor(this, label ?? "", memberName ?? "", FormatPath(filePath) ?? "", line);
    }

    [Event(9, Level = EventLevel.Informational, Keywords = Keywords.Logging, Message = "[{0}][{2}:{3}][{1}]{4}ms")]
    void MeasureExecution(string label, string memberName, string filePath, int line, double duration)
    {
        WriteEvent(9, label ?? "", memberName ?? "", FormatPath(filePath) ?? "", line, duration);
    }

    class StopwatchMonitor : IDisposable
    {
        readonly LoggerEventSource logger;
        readonly string label;
        readonly string memberName;
        readonly string filePath;
        readonly int line;
        Stopwatch stopwatch;

        public StopwatchMonitor(LoggerEventSource logger, string label, string memberName, string filePath, int line)
        {
            this.logger = logger;
            this.label = label;
            this.memberName = memberName;
            this.filePath = filePath;
            this.line = line;
            stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (stopwatch != null)
            {
                stopwatch.Stop();
                logger.MeasureExecution(label, memberName, filePath, line, stopwatch.Elapsed.TotalMilliseconds);
                stopwatch = null;
            }
        }
    }
}
```

This is basic definition. Next step, you should define own method for structured-logging.

How to save log to file? Where is Out-of-Process Service?
---
EtwStream currently no provides FileSink, Out-of-Process Service. Please use [Semantic Logging Application Block](https://msdn.microsoft.com/en-us/library/dn440729.aspx) or [Serilog](http://serilog.net/).

But I will create new style of Out-of-Process Service configuration with Roslyn C# Scripting API and sinks.  - https://github.com/neuecc/EtwStream/issues/2
Please wait a moment.

Wellknown EventSources
---
Following Guid is providing by `EtwStream.WellKnownEventSources`.

* [System.Diagnostics.Eventing.FrameworkEventSource](http://referencesource.microsoft.com/#mscorlib/system/diagnostics/eventing/frameworkeventsource.cs,33) - 8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1
* [System.Collections.Concurrent.ConcurrentCollectionsEventSource](http://referencesource.microsoft.com/#mscorlib/system/Collections/Concurrent/CDSCollectionETWBCLProvider.cs,30) - 35167F8E-49B2-4b96-AB86-435B59336B5E
* [System.Threading.SynchronizationEventSource](http://referencesource.microsoft.com/#mscorlib/system/threading/CDSsyncETWBCLProvider.cs,32) - EC631D38-466B-4290-9306-834971BA0217 
* [System.Threading.Tasks.TplEventSource](http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/TPLETWProvider.cs,28) - 2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5         
* [PinnableBufferCacheEventSource](http://referencesource.microsoft.com/#System/parent/parent/parent/InternalApis/NDP_Common/inc/PinnableBufferCache.cs,598) - Microsoft-DotNETRuntime-PinnableBufferCache-System        
* [System.Linq.Parallel.PlinqEventSource](http://referencesource.microsoft.com/#System.Core/System/Linq/Parallel/Utils/PLINQETWProvider.cs,28) - 159eeeec-4a14-4418-a8fe-faabcd987887
* [SqlEventSource](http://referencesource.microsoft.com/#System.Data/System/Data/Common/SqlEventSource.cs,13) - Microsoft-AdoNet-SystemData
* [Microsoft-Windows-ASPNET](http://referencesource.microsoft.com/#System.Web/AspNetEventSource.cs,22) - ee799f41-cfa5-550b-bf2c-344747c1c668    
    
IIS 8.5 Logging to ETW     
---
Following Guid is providing by `EtwStream.IISEventSources`.

* Microsoft-Windows-HttpEvent - 7B6BC78C-898B-4170-BBF8-1A469EA43FC5
* Microsoft-Windows-HttpLog - C42A2738-2333-40A5-A32F-6ACC36449DCC
* Microsoft-Windows-HttpService - DD5EF90A-6398-47A4-AD34-4DCECDEF795F
* Microsoft-Windows-Runtime-Web-Http - 41877CB4-11FC-4188-B590-712C143C881D
* Microsoft-Windows-Runtime-WebAPI - 6BD96334-DC49-441A-B9C4-41425BA628D8
* ASP.NET Events - AFF081FE-0247-4275-9C4E-021F3DC1DA35
* Microsoft-Windows-IIS-APPHOSTSVC - CAC10856-9223-48FE-96BA-2A772274FB53
* Microsoft-Windows-IIS-Logging - 7E8AD27F-B271-4EA2-A783-A47BDE29143B
* Microsoft-Windows-IIS-W3SVC - 05448E22-93DE-4A7A-BBA5-92E27486A8BE
