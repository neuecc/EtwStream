using System;

namespace EtwStream
{
    public interface IObservableEventListener<T> : IObservable<T>, IDisposable
    {
    }
}
