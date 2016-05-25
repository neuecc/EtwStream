using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public static class SinkExtensions
    {
        public static IDisposable LogTo<T>(this IObservable<T> source, Func<IObservable<T>, IDisposable[]> subscribe)
        {
            var publishedSource = source.Publish().RefCount();
            var subscriptions = subscribe(publishedSource);
            return new System.Reactive.Disposables.CompositeDisposable(subscriptions);
        }
    }
}