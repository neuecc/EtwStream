using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwStream
{
    public interface ISubscriptionContainer
    {
        void Add(IDisposable subscription);
    }

    public class SubscriptionContainer : ISubscriptionContainer
    {
        readonly CompositeDisposable subscriptions = new CompositeDisposable();

        public void Add(IDisposable subscription)
        {
            subscriptions.Add(subscription);
        }

        public void Dispose()
        {
            subscriptions.Dispose();
        }
    }

    public static class EtwStreamSubscriptionContainerExtensions
    {
        public static void AddTo(this IDisposable subscription, ISubscriptionContainer container)
        {
            container.Add(subscription);
        }
    }
}