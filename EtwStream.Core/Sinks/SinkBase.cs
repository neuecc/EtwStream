using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public abstract class SinkBase<T> : IObserver<T>, IObserver<IList<T>>
    {
        public SinkBase()
        {

        }

        public abstract void OnNext(IList<T> value);
        public abstract void Flush();

        public virtual void OnNext(T value)
        {
            OnNext(new[] { value });
        }

        public virtual void OnError(Exception error)
        {
            Flush();
        }

        public virtual void OnCompleted()
        {
            Flush();
        }
    }
}