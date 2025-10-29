using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class InMemoryStorageBackend : IStorageBackend
    {
        private readonly Dictionary<FilePath, byte[]> _data = new();
        
        private readonly SemaphoreSlim _locker = new(1, 1);
        
        public event Action<Exception>? OnError;

        public async Task<bool> Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            await _locker.WaitAsync();
            try
            {
                MemoryStream stream = new();
                await writer(stream);
                _data[path] = stream.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
            finally
            {
                _locker.Release();    
            }
        }

        public async Task<bool> Read(FilePath path, Func<Stream, Task> reader)
        {
            await _locker.WaitAsync();
            try
            {
                if (_data.TryGetValue(path, out var bytes))
                {
                    var stream = new MemoryStream(bytes, false);
                    stream.Position = 0;
                    await reader(stream);
                    return true;
                }

                throw new KeyNotFoundException(path.ToString());
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<bool> Erase(FilePath path)
        {
            await _locker.WaitAsync();
            try
            {
                return _data.Remove(path);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<bool> IsExists(FilePath path)
        {
            await _locker.WaitAsync();
            try
            {
                return _data.ContainsKey(path);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            await _locker.WaitAsync();
            try
            {
                List<FilePath> list = new();

                foreach (var p in _data.Keys)
                {
                    if (path.ContainsFile(p))
                    {
                        list.Add(p);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return [];
            }
            finally
            {
                _locker.Release();
            }
        }
    }
}