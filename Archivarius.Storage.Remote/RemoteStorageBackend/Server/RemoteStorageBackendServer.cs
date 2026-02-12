using System;
using Pontifex.Abstractions.Servers;
using Pontifex.Api;

namespace Archivarius.Storage.Remote
{
    public class RemoteStorageBackendServer
    {
        private readonly IReadOnlySyncStorageBackend _roStorage;
        private readonly ISyncStorageBackend? _writeStorage;

        public event Action<IServerSideStorageApiInstance>? Started;
        public event Action<IServerSideStorageApiInstance>? Stopped;

        public RemoteStorageBackendServer(ISyncStorageBackend storage)
        {
            _roStorage = _writeStorage = storage;
        }
        
        public RemoteStorageBackendServer(IReadOnlySyncStorageBackend storage)
        {
            _roStorage = storage;
        }
        
        public bool Setup(IAckRawServer transport)
        {
            transport.Init(new ServerSideApiFactory<RemoteStorageApi>(
                _ =>
                {
                    var res = _writeStorage != null ? 
                        new ServerSideStorageApiInstance(_writeStorage, transport.Memory, transport.Log) : 
                        new ServerSideStorageApiInstance(_roStorage, transport.Memory, transport.Log);
                    res.ApiStarted += i => Started?.Invoke((ServerSideStorageApiInstance)i);
                    res.ApiStopped += i => Stopped?.Invoke((ServerSideStorageApiInstance)i);
                    return res;
                }));
            return transport.Start(_ => { });
        }
    }
}