using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Internals
{
    public class ObjectPool<T>
    {
        private readonly Func<T> _ctor;
        private readonly Action<T> _resetMethod;

        private readonly ConcurrentBag<T> _objects1 = new();

        public ObjectPool(Func<T> ctor, Action<T> resetMethod)
        {
            _ctor = ctor;
            _resetMethod = resetMethod;
        }

        public T Get()
        {
            if (_objects1.TryTake(out var pool))
            {
                return pool;
            }
            return _ctor.Invoke();
        }
        
        public void Release(T value)
        {
            _resetMethod.Invoke(value);
            _objects1.Add(value);
        }
    }
}