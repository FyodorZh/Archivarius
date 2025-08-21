using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class FsStorageBackend : IStorageBackend
    {
        private readonly string _root;

        public FsStorageBackend(string root)
        {
            if (root.EndsWith("/"))
            {
                throw new InvalidOperationException($"{root} is not valid");
            }
            _root = root;
        }

        public async Task Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            using var file = File.Open(_root + path, FileMode.Create, FileAccess.Write);
            await writer(file);
        }

        public async Task Read(FilePath path, Func<Stream, Task> reader)
        {
            using var file = File.Open(_root + path, FileMode.Open, FileAccess.Read);
            await reader.Invoke(file);
        }

        public Task Erase(FilePath path)
        {
            string filePath = _root + path;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsExists(FilePath path)
        {
            string filePath = _root + path;
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            var dir = _root + path;
            if (!Directory.Exists(dir))
            {
                return Task.FromResult<IReadOnlyCollection<FilePath>>([]);
            }

            int rootDirLength = _root.Length;

            PathFactory factory = new();

            var list = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Select(file =>
            {
                file = file.Substring(rootDirLength);
                return (FilePath)factory.BuildWithCache(file);
            });

            return Task.FromResult<IReadOnlyCollection<FilePath>>(list.ToArray());
        }
    }
}