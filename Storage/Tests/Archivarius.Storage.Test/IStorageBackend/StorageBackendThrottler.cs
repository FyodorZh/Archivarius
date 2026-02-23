using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test
{
    public class StorageBackendThrottler : IStorageBackend
    {
        private readonly IStorageBackend _backend;
        private readonly TimeSpan _delay;

        public event Action<Exception> OnError
        {
            add => _backend.OnError += value;
            remove => _backend.OnError -= value;
        }

        public bool ThrowExceptions
        {
            get => _backend.ThrowExceptions;
            set => _backend.ThrowExceptions = value;
        }

        public StorageBackendThrottler(IStorageBackend backend, TimeSpan delay)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
            _delay = delay;
        }

        public async Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
        {
            await Task.Delay(_delay);
            return await _backend.Read(path, param, reader);
        }

        public async Task<bool> IsExists(FilePath path)
        {
            await Task.Delay(_delay);
            return await _backend.IsExists(path);
        }

        public async Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            await Task.Delay(_delay);
            return await _backend.GetSubPaths(path);
        }

        public async Task<bool> Write<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> writer)
        {
            await Task.Delay(_delay);
            return await _backend.Write(path, param, writer);
        }

        public async Task<bool> Erase(FilePath path)
        {
            await Task.Delay(_delay);
            return await _backend.Erase(path);
        }
    }
}
