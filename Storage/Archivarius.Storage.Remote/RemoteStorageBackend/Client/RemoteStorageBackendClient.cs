using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Actuarius.Memory;
using Archivarius.DataModels;
using Pontifex.Abstractions.Clients;
using Pontifex.Api;
using Pontifex.Api.Client;

namespace Archivarius.Storage.Remote
{
    public class RemoteStorageBackendClient : IStorageBackend
    {
        private readonly RemoteStorageApi _api;
        private readonly IAckRawClient _transport;
        private readonly bool _isReadOnly;

        private readonly IMemoryRental _memoryRental;
        private readonly IConcurrentPool<MemoryStream> _memoryStreamPool;
        
        public bool ThrowExceptions { get; set; }

        public event Action<Exception>? OnError;

        public static IReadOnlyStorageBackend? ConstructRO(IAckRawClient transport)
        {            
            var api = new RemoteStorageApi();
            if (!transport.Init(new ClientSideApi(api, transport.Memory, transport.Log)))
            {
                return null;
            }
            if (!transport.Start(stopReason => { }))
            {
                return null;
            }
            
            return new RemoteStorageBackendClient(api, transport, false);
        }
        
        public static IStorageBackend? Construct(IAckRawClient transport)
        {
            var api = new RemoteStorageApi();
            if (!transport.Init(new ClientSideApi(api, transport.Memory, transport.Log)))
            {
                return null;
            }
            if (!transport.Start(_ => { }))
            {
                return null;
            }
            
            return new RemoteStorageBackendClient(api, transport, false);
        }
        
        private RemoteStorageBackendClient(RemoteStorageApi api, IAckRawClient transport, bool isReadOnly)
        {
            _api = api;
            _transport = transport;
            _isReadOnly = isReadOnly;
            _memoryRental = transport.Memory;
            _memoryStreamPool = _memoryRental.BigObjectsPool.GetPool<MemoryStream>();
        }

        public async Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
        {
            try
            {
                var res = await _api.Read.RequestAsync(new StringWrapper(path.FullName), TimeSpan.FromDays(1));
                if (res.Value == null) 
                    return false;
                try
                {
                    MemoryStream streamToRead = new MemoryStream(res.Value.Array, res.Value.Offset, res.Value.Count, false, false);
                    await reader.Invoke(streamToRead, param);
                }
                finally
                {
                    res.Value.Release();
                }

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                {
                    throw;
                }
                return false;
            }
        }

        public async Task<bool> IsExists(FilePath path)
        {
            try
            {
                var res = await _api.IsExists.RequestAsync(new StringWrapper(path.FullName), TimeSpan.FromDays(1));
                return res.Value;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                {
                    throw;
                }
                return false;
            }
        }

        public async Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            try
            {
                var res = await _api.GetSubPath.RequestAsync(new StringWrapper(path.FullName), TimeSpan.FromDays(1));
                if (res.Value == null)
                    return Array.Empty<FilePath>();
                return res.Value.Select(s => PathFactory.BuildFile(s.Value!)).ToArray();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                {
                    throw;
                }
                return Array.Empty<FilePath>();
            }
        }
        
        public async Task<bool> Write<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> writer)
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException();
            }

            MemoryStream ms = _memoryStreamPool.Acquire();
            try
            {
                await writer.Invoke(ms, param);
                IMultiRefByteArray bytes = _memoryRental.ByteArraysPool.Acquire((int)ms.Length);
                try
                {
                    ms.Position = 0;
                    var _ = ms.Read(bytes.Array, bytes.Offset, bytes.Count);
                    var res = await _api.Write.RequestAsync(new Pair<StringWrapper, MultiRefByteArrayWrapper>()
                    {
                        First = new StringWrapper(path.FullName),
                        Second = new MultiRefByteArrayWrapper() { Value = bytes }
                    }, TimeSpan.FromDays(1));
                    return res.Value;
                }
                finally
                {
                    bytes.Release();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                {
                    throw;
                }

                return false;
            }
            finally
            {
                ms.SetLength(0);
                _memoryStreamPool.Release(ms);
            }
        }

        public async Task<bool> Erase(FilePath path)
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException();
            }

            try
            {
                var res = await _api.Delete.RequestAsync(new StringWrapper() { Value = path.FullName }, TimeSpan.FromDays(1));
                return res.Value;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                {
                    throw;
                }
                return false;
            }
        }
    }
}