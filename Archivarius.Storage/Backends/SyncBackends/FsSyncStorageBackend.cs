using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class FsSyncStorageBackend : ISyncStorageBackend
    {
        private readonly string _root;

        public event Action<Exception>? OnError;

        public bool ThrowExceptions { get; set; } = true;

        public FsSyncStorageBackend(string root)
        {
            if (root.EndsWith("/"))
            {
                throw new InvalidOperationException($"{root} is not valid");
            }
            _root = root;
        }

        public bool Write<TParam>(FilePath path, TParam param, Action<Stream, TParam> writer)
        {
            try
            {
                string filePath = _root + path;
                string dirPath = _root + path.Parent;

                Directory.CreateDirectory(dirPath);

                if (Directory.Exists(filePath))
                {
                    if (Directory.EnumerateFiles(filePath, "*", SearchOption.AllDirectories).Any())
                    {
                        return false;
                    }
                    Directory.Delete(filePath, true);
                }

                using var file = File.Open(filePath, FileMode.Create, FileAccess.Write);
                writer(file, param);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public bool Read<TParam>(FilePath path, TParam param, Action<Stream, TParam> reader)
        {
            try
            {
                string filePath = _root + path;
                if (File.Exists(filePath))
                {
                    using var file = File.Open(filePath, FileMode.Open, FileAccess.Read);
                    reader.Invoke(file, param);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public bool Erase(FilePath path)
        {
            try
            {
                string filePath = _root + path;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public bool IsExists(FilePath path)
        {
            string filePath = _root + path;
            return File.Exists(filePath);
        }

        public IReadOnlyList<FilePath> GetSubPaths(DirPath path)
        {
            try
            {
                var dir = _root + path;
                if (!Directory.Exists(dir))
                {
                    return [];
                }

                int rootDirLength = _root.Length;

                PathFactory factory = new();

                var list = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Select(file =>
                {
                    file = file.Substring(rootDirLength);
                    return (FilePath)factory.BuildWithCache(file);
                }).ToList();

                list.Sort();

                return list;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return [];
            }
        }
    }
}