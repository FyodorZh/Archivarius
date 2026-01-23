using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Actuarius.Memory;
using Archivarius.DataModels;
using Pontifex.Abstractions.Servers;
using Pontifex.Api;
using Pontifex.Api.Server;
using Scriba;

namespace Archivarius.Storage.Remote
{
    public class RemoteStorageBackendServer
    {
        private readonly IReadOnlyStorageBackend _roStorage;
        private readonly IStorageBackend? _writeStorage;
        
        public event Action<RemoteStorageCommandInfo>? OnCommand;

        public RemoteStorageBackendServer(IStorageBackend storage)
        {
            _roStorage = _writeStorage = storage;
        }
        
        public RemoteStorageBackendServer(IReadOnlyStorageBackend storage)
        {
            _roStorage = storage;
        }
        
        public bool Setup(IAckRawServer transport)
        {
            transport.Init(new ServerSideApiFactory<RemoteStorageApi>(
                _ =>
                {
                    var res = _writeStorage != null ? 
                        new ServerSideStorageApi(_writeStorage, transport.Memory, transport.Log) : 
                        new ServerSideStorageApi(_roStorage, transport.Memory, transport.Log);
                    res.OnCommand += OnCommand;
                    return res;
                }));
            return transport.Start(_ => { });
        }

        private class ServerSideStorageApi : ServerSideApi<RemoteStorageApi>
        {
            private readonly IReadOnlyStorageBackend _roStorage;
            private readonly IStorageBackend? _writeStorage;
            private readonly IConcurrentPool<MemoryStream> _memoryStreamsPool;

            public event Action<RemoteStorageCommandInfo>? OnCommand;
            
            public ServerSideStorageApi(IReadOnlyStorageBackend storage, IMemoryRental memoryRental, ILogger logger) 
                : base(new RemoteStorageApi(), memoryRental, logger)
            {
                storage.ThrowExceptions = true;
                _roStorage = storage;
                _writeStorage = storage as IStorageBackend;
            
                _memoryStreamsPool = memoryRental.BigObjectsPool.GetPool<MemoryStream>();
                
                Api.IsExists.SetProcessorAsync(IsExists);
                Api.GetSubPath.SetProcessorAsync(GetSubPath);
                Api.Read.SetProcessorAsync(Read);
                Api.Write.SetProcessorAsync(Write);
                Api.Delete.SetProcessorAsync(Delete);
            }

            private async Task<BoolWrapper> IsExists(StringWrapper request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("IsExists");
                try
                {
                    var filePath = PathFactory.BuildFile(request.Value ?? throw new Exception("FilePath is null"));
                    var res = await _roStorage.IsExists(filePath);
                    info.Finish(true);
                    return new BoolWrapper(res);
                }
                catch
                {
                    info.Finish(false);
                    throw;
                }
                finally
                {
                    OnCommand?.Invoke(info);
                }
            }

            private async Task<StructsArray<StringWrapper>> GetSubPath(StringWrapper request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("GetSubPaths");
                try
                {
                    var dirPath = PathFactory.BuildDir(request.Value ?? throw new Exception("DirPath is null"));
                    var res = await _roStorage.GetSubPaths(dirPath);
                    info.Finish(true);
                    return new StructsArray<StringWrapper>(res.Select(fPath => new StringWrapper(fPath.FullName)).ToArray());
                }
                catch
                {
                    info.Finish(false);
                    throw;
                }
                finally
                {
                    OnCommand?.Invoke(info);
                }
            }

            private async Task<BytesWrapper> Read(StringWrapper request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Read");
                var memoryStream = _memoryStreamsPool.Acquire();
                try
                {
                    var filePath = PathFactory.BuildFile(request.Value ?? throw new Exception("FilePath is null"));
                    var res = await _roStorage.Read(filePath, memoryStream, (stream, dst) => stream.CopyToAsync(dst));
                    if (!res)
                    {
                        info.Finish(false);
                        return new BytesWrapper(null);
                    }

                    var bytes = memoryStream.ToArray();
                    info.Finish(true);
                    return new BytesWrapper(bytes);
                }
                catch
                {
                    info.Finish(false);
                    throw;
                }
                finally
                {
                    memoryStream.SetLength(0);
                    _memoryStreamsPool.Release(memoryStream);
                    OnCommand?.Invoke(info);
                }
            }

            private async Task<BoolWrapper> Write(Pair<StringWrapper, BytesWrapper> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Write");
                try
                {
                    if (_writeStorage == null)
                    {
                        throw new Exception("Write is not supported");
                    }

                    var bytes = request.Second.Value;
                    if (bytes == null)
                    {
                        throw new Exception("Data is null");
                    }

                    var filePath = PathFactory.BuildFile(request.First.Value ?? throw new Exception("FilePath is null"));
                    var res = await _writeStorage.Write(filePath, bytes, (stream, src) => stream.WriteAsync(src, 0, src.Length));
                    info.Finish(res);
                    return new BoolWrapper(res);
                }
                catch
                {
                    info.Finish(false);
                    throw;
                }
                finally
                {
                    OnCommand?.Invoke(info);
                }
            }

            private async Task<BoolWrapper> Delete(StringWrapper request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Delete");
                try
                {
                    if (_writeStorage == null)
                    {
                        throw new Exception("Delete is not supported");
                    }

                    var filePath = PathFactory.BuildFile(request.Value ?? throw new Exception("FilePath is null"));
                    var res = await _writeStorage.Erase(filePath);
                    info.Finish(res);
                    return new BoolWrapper(res);
                }
                catch
                {
                    info.Finish(false);
                    throw;
                }
                finally
                {
                    OnCommand?.Invoke(info);
                }
            }
        }
    }
}