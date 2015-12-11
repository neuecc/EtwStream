ObservableEventListener.ClearAllActiveObservableEventListenerSession();

ObservableEventListener.FromTraceEvent(WellKnownEventSources.TplEventSource)
    .TakeUntil(EtwStreamService.TerminateToken)
    .LogToConsoleAsync()
    .AddTo(EtwStreamService.TaskContainer);