using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class ReadOnlyDirectoryStorageBackend : IReadOnlyStorageBackend
    {
        private readonly IReadOnlyStorageBackend _storage;
        protected readonly DirPath _path;
        
        public event Action<Exception>? OnError; 

        public ReadOnlyDirectoryStorageBackend(IReadOnlyStorageBackend storage, DirPath dir)
        {
            if (storage is ReadOnlyDirectoryStorageBackend roDirBackend)
            {
                _storage = roDirBackend._storage;
                _path = roDirBackend._path.Dir(dir);
            }
            else
            {
                _storage = storage;
                _path = dir;
            }
            storage.OnError += e => OnError?.Invoke(e);
        }
        
        public Task<bool> Read(FilePath path, Func<Stream, Task> reader)
        {
            return _storage.Read(_path.File(path), reader);
        }

        public Task<bool> IsExists(FilePath path)
        {
            return _storage.IsExists(_path.File(path));
        }

        public Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            return _storage.GetSubPaths(_path.Dir(path));
        }
    }
    public class DirectoryStorageBackend : ReadOnlyDirectoryStorageBackend, IStorageBackend
    {
        private readonly IStorageBackend _storage;

        public DirectoryStorageBackend(IStorageBackend storage, DirPath dir)
            : base((storage is DirectoryStorageBackend dirBackend1) ? dirBackend1._storage : storage,
                (storage is DirectoryStorageBackend dirBackend2) ? dirBackend2._path.Dir(dir) : dir)
        {
            _storage = (storage is DirectoryStorageBackend dirBackend) ? dirBackend._storage : storage;
        }
        
        public Task<bool> Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            return _storage.Write(_path.File(path), writer);
        }

        public Task<bool> Erase(FilePath path)
        {
            return _storage.Erase(_path.File(path));
        }
    }

    public static class DirectoryStorageBackend_Ext
    {
        public static IReadOnlyStorageBackend SubDirectory(this IReadOnlyStorageBackend storage, DirPath subDirectory)
        {
            return new ReadOnlyDirectoryStorageBackend(storage, subDirectory);
        }
        
        public static IStorageBackend SubDirectory(this IStorageBackend storage, DirPath subDirectory)
        {
            return new DirectoryStorageBackend(storage, subDirectory);
        }
    }
}