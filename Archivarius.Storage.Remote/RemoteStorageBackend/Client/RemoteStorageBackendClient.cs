using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            if (!transport.Start(stopReason => { }))
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
        }

        public async Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
        {
            try
            {
                var res = await _api.Read.RequestAsync(new StringWrapper(path.FullName));
                if (res.Value == null) 
                    return false;
                await reader.Invoke(new MemoryStream(res.Value), param);
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
                var res = await _api.IsExists.RequestAsync(new StringWrapper(path.FullName));
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
                var res = await _api.GetSubPath.RequestAsync(new StringWrapper(path.FullName));
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
            
            try
            {
                MemoryStream ms = new MemoryStream();
                await writer.Invoke(ms, param);
                var res = await _api.Write.RequestAsync(new Pair<StringWrapper, BytesWrapper>()
                {
                    First = new StringWrapper(path.FullName),
                    Second = new BytesWrapper(ms.ToArray())
                });
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

        public async Task<bool> Erase(FilePath path)
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException();
            }

            try
            {
                var res = await _api.Delete.RequestAsync(new StringWrapper() { Value = path.FullName });
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