ObservableEventListener.ClearAllActiveObservableEventListenerSession();

var t1 = ObservableEventListener.FromTraceEvent(WellKnownEventSources.TplEventSource)
    .TakeUntil(EtwStreamService.TerminateToken)
    .LogToConsoleAsync();

EtwStreamService.CompleteConfiguration(t1)