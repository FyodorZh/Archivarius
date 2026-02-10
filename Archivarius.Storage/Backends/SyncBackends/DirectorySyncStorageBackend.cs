using System;
using System.Collections.Generic;
using System.IO;

namespace Archivarius.Storage
{
    public class ReadOnlyDirectorySyncStorageBackend : IReadOnlySyncStorageBackend
    {
        private readonly IReadOnlySyncStorageBackend _storage;
        protected readonly DirPath _path;
        
        public event Action<Exception>? OnError;

        public bool ThrowExceptions
        {
            get => _storage.ThrowExceptions;
            set => _storage.ThrowExceptions = value;
        }

        public ReadOnlyDirectorySyncStorageBackend(IReadOnlySyncStorageBackend storage, DirPath dir)
        {
            if (storage is ReadOnlyDirectorySyncStorageBackend roDirBackend)
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
        
        public bool Read<TParam>(FilePath path, TParam param, Action<Stream, TParam> reader)
        {
            return _storage.Read(_path.File(path), param, reader);
        }

        public bool IsExists(FilePath path)
        {
            return _storage.IsExists(_path.File(path));
        }

        public IReadOnlyList<FilePath> GetSubPaths(DirPath path)
        {
            return _storage.GetSubPaths(_path.Dir(path));
        }
    }
    
    public class DirectorySyncStorageBackend : ReadOnlyDirectorySyncStorageBackend, ISyncStorageBackend
    {
        private readonly ISyncStorageBackend _storage;

        public DirectorySyncStorageBackend(ISyncStorageBackend storage, DirPath dir)
            : base((storage is DirectorySyncStorageBackend dirBackend1) ? dirBackend1._storage : storage,
                (storage is DirectorySyncStorageBackend dirBackend2) ? dirBackend2._path.Dir(dir) : dir)
        {
            _storage = (storage is DirectorySyncStorageBackend dirBackend) ? dirBackend._storage : storage;
        }
        
        public bool Write<TParam>(FilePath path, TParam param, Action<Stream, TParam> writer)
        {
            return _storage.Write(_path.File(path), param, writer);
        }

        public bool Erase(FilePath path)
        {
            return _storage.Erase(_path.File(path));
        }
    }

    public static class DirectorySyncStorageBackend_Ext
    {
        public static IReadOnlySyncStorageBackend SubDirectory(this IReadOnlySyncStorageBackend storage, DirPath subDirectory)
        {
            return new ReadOnlyDirectorySyncStorageBackend(storage, subDirectory);
        }
        
        public static ISyncStorageBackend SubDirectory(this ISyncStorageBackend storage, DirPath subDirectory)
        {
            return new DirectorySyncStorageBackend(storage, subDirectory);
        }
    }
}