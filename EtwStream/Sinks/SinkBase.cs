using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public abstract class SinkBase<T> : IObserver<T>, IObserver<IList<T>>, IDisposable
    {
        public SinkBase()
        {

        }

        public abstract void OnNext(IList<T> value);
        public abstract void Dispose();

        public virtual void OnNext(T value)
        {
            OnNext(new[] { value });
        }

        public virtual void OnError(Exception error)
        {
            Dispose();
        }

        public virtual void OnCompleted()
        {
            Dispose();
        }

        public IDisposable CreateLinkedDisposable(IDisposable subscription)
        {
            //stop subscription first, after flush self.
            return new BinaryCompositeDisposable(subscription, this);
        }

        class BinaryCompositeDisposable : IDisposable
        {
            private volatile IDisposable disposable1;
            private volatile IDisposable disposable2;

            public BinaryCompositeDisposable(IDisposable disposable1, IDisposable disposable2)
            {
                this.disposable1 = disposable1;
                this.disposable2 = disposable2;
            }

            public void Dispose()
            {
#pragma warning disable 0420
                var old1 = System.Threading.Interlocked.Exchange(ref disposable1, null);
#pragma warning restore 0420
                if (old1 != null)
                {
                    old1.Dispose();
                }

#pragma warning disable 0420
                var old2 = System.Threading.Interlocked.Exchange(ref disposable2, null);
#pragma warning restore 0420
                if (old2 != null)
                {
                    old2.Dispose();
                }
            }
        }
    }
}