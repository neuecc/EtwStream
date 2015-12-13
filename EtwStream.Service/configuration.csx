// ClearAllActiveObservableEventListenerSession is clear all trace event session.
// Basicaly EtwStream.Service terminate session after finished but if rest sessions, useful.
// ObservableEventListener.ClearAllActiveObservableEventListenerSession();

// Open multiple session is not recommended, use Publish().RefCount() is best way.
var source = ObservableEventListener.FromTraceEvent(WellKnownEventSources.TplEventSource).Publish().RefCount();

// Buffer(TermintateToken) or TakeUntil(TerminateToken) must needs for normal termination.
source.Buffer(TimeSpan.FromSeconds(1), 1000, EtwStreamService.TerminateToken)
    .LogTo(xs =>
    {
        // write to text and write to console.
        var d1 = xs.LogToFile(@"test.txt", x => x.DumpPayloadOrMessage(), Encoding.UTF8, false);
        var d2 = xs.LogToConsole();
        return new[] { d1, d2 };
    })
    .AddTo(EtwStreamService.Container); // AddTo(Container) must needs for normal termination.