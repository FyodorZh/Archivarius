using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class DirectoryStorageBackend : IStorageBackend
    {
        private readonly IStorageBackend _storage;
        private readonly DirPath _path;

        public DirectoryStorageBackend(IStorageBackend storage, DirPath dir)
        {
            if (storage is DirectoryStorageBackend dirBackend)
            {
                _storage = dirBackend._storage;
                _path = dirBackend._path.Dir(dir);
            }
            else
            {
                _storage = storage;
                _path = dir;
            }
        }
        
        public Task Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            return _storage.Write(_path.File(path), writer);
        }

        public Task Read(FilePath path, Func<Stream, Task> reader)
        {
            return _storage.Read(_path.File(path), reader);
        }

        public Task Erase(FilePath path)
        {
            return _storage.Erase(_path.File(path));
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

    public static class DirectoryStorageBackend_Ext
    {
        public static IStorageBackend SubDirectory(this IStorageBackend storage, DirPath subDirectory)
        {
            return new DirectoryStorageBackend(storage, subDirectory);
        }
    }
}