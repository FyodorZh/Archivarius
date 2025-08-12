using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Internals
{
    public class ObjectPool<T>
    {
        private readonly Func<T> _ctor;
        private readonly Action<T> _resetMethod;
        
        private readonly Stack<T> _objects = new();
        private readonly SemaphoreSlim _locker = new(1, 1);

        public ObjectPool(Func<T> ctor, Action<T> resetMethod)
        {
            _ctor = ctor;
            _resetMethod = resetMethod;
        }

        public T Get()
        {
            _locker.Wait();
            try
            {
                if (_objects.Count > 0)
                {
                    return _objects.Pop();
                }
            }
            finally
            {
                _locker.Release();
            }
            return _ctor.Invoke();
        }

        public async Task<T> GetAsync()
        {
            await _locker.WaitAsync();
            try
            {
                if (_objects.Count > 0)
                {
                    return _objects.Pop();
                }
            }
            finally
            {
                _locker.Release();
            }
            return _ctor.Invoke();
        }
        
        public void Release(T value)
        {
            _resetMethod.Invoke(value);
            _locker.Wait();
            _objects.Push(value);
            _locker.Release();
        }

        public async Task ReleaseAsync(T value)
        {
            _resetMethod.Invoke(value);
            await _locker.WaitAsync();
            _objects.Push(value);
            _locker.Release();
        }
    }
}