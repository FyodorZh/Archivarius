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

        public event Action<Exception>? OnError; 

        public FsStorageBackend(string root)
        {
            if (root.EndsWith("/"))
            {
                throw new InvalidOperationException($"{root} is not valid");
            }
            _root = root;
        }

        public async Task<bool> Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            try
            {
                string filePath = _root + path;
                string dirPath = _root + path.Parent;

                Directory.CreateDirectory(dirPath);

                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write);
                await writer(file);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public async Task<bool> Read(FilePath path, Func<Stream, Task> reader)
        {
            try
            {
                using var file = File.Open(_root + path, FileMode.Open, FileAccess.Read);
                await reader.Invoke(file);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        public Task<bool> Erase(FilePath path)
        {
            try
            {
                string filePath = _root + path;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsExists(FilePath path)
        {
            string filePath = _root + path;
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            try
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
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return Task.FromResult<IReadOnlyCollection<FilePath>>([]);
            }
        }
    }
}