using System;
using System.Linq;
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
        private readonly IReadOnlySyncStorageBackend _roStorage;
        private readonly ISyncStorageBackend? _writeStorage;

        public event Action<double>? WorkLoad;
        
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
                        new ServerSideStorageApi(_writeStorage, transport.Memory, transport.Log) : 
                        new ServerSideStorageApi(_roStorage, transport.Memory, transport.Log);
                    res.WorkLoad += (w) => WorkLoad?.Invoke(w);
                    return res;
                }));
            return transport.Start(_ => { });
        }

        private class ServerSideStorageApi : ServerSideApi<RemoteStorageApi>
        {
            private readonly StorageBackendLogic<UserData> _storage;
            
            public event Action<double>? WorkLoad;
            
            public ServerSideStorageApi(IReadOnlySyncStorageBackend storage, IMemoryRental memoryRental, ILogger logger) 
                : base(new RemoteStorageApi(), memoryRental, logger)
            {
                double worlkload = 0;
                if (storage is ISyncStorageBackend wStorage)
                {
                    _storage = new StorageBackendLogic<UserData>(wStorage, memoryRental, w =>
                    {
                        worlkload = worlkload * 0.95 + w * 0.05;
                        WorkLoad?.Invoke(worlkload);
                    } );
                }
                else
                {
                    _storage = new StorageBackendLogic<UserData>(storage, memoryRental, w =>
                    {
                        worlkload = worlkload * 0.95 + w * 0.05;
                        WorkLoad?.Invoke(worlkload);
                    } );
                }

                Api.Disconnected += _ =>
                {
                    _storage.Dispose();
                };
                
                Api.IsExists.SetProcessor(IsExists);
                Api.GetSubPath.SetProcessor(GetSubPath);
                Api.Read.SetProcessor(Read);
                Api.Write.SetProcessor(Write);
                Api.Delete.SetProcessor(Delete);
            }

            private void IsExists(Request<StringWrapper, BoolWrapper> request)
            {
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData();
                userData.IsExistsRequest = request;
                _storage.IsExists(filePath, userData, 
                    (res, data) => data.OK_IsExistsRequest(new BoolWrapper(res)),
                    (e, data) => data.Fail_IsExistsRequest(e));
            }

            private void GetSubPath(Request<StringWrapper, StructsArray<StringWrapper>> request)
            {
                DirPath dirPath;
                try
                {
                    dirPath = PathFactory.BuildDir(request.Data.Value ?? throw new Exception("DirPath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    return;
                }

                var userData = _storage.GetFreeUserData() ?? new UserData();
                userData.GetSubPathsRequest = request;
                _storage.GetSubPath(dirPath, userData, 
                    (res, data) =>
                    {
                        var model = new StructsArray<StringWrapper>(res.Select(fPath => new StringWrapper(fPath.FullName)).ToArray());
                        data.OK_GetSubPathsRequest(model);
                    },
                    (e, data) => data.Fail_GetSubPathsRequest(e));
            }

            private void Read(Request<StringWrapper, MultiRefByteArrayWrapper> request)
            {
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    return;
                }

                var userData = _storage.GetFreeUserData() ?? new UserData();
                userData.ReadRequest = request;
                _storage.Read(filePath, userData, 
                    (res, data) => data.OK_ReadRequest(res),
                    (e, data) => data.Fail_ReadRequest(e));
            }

            private void Write(Request<Pair<StringWrapper, MultiRefByteArrayWrapper>, BoolWrapper> request)
            {
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.First.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    return;
                }

                if (request.Data.Second.Value == null)
                {
                    request.Fail("Nothing to write");
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData();
                userData.WriteRequest = request;
                _storage.Write(filePath, request.Data.Second.Value, userData, 
                    (res, data) => data.OK_WriteRequest(new BoolWrapper(res)), 
                    (e, data) => data.Fail_WriteRequest(e));
            }

            private void Delete(Request<StringWrapper, BoolWrapper> request)
            {
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData();
                userData.EraseRequest = request;
                _storage.Erase(filePath, userData, 
                    (res, data) => data.OK_EraseRequest(new BoolWrapper(res)), 
                    (e, data) => data.Fail_EraseRequest(e));
            }

            private class UserData
            {
                public Request<StringWrapper, BoolWrapper>? IsExistsRequest;
                public Request<StringWrapper, StructsArray<StringWrapper>>? GetSubPathsRequest;
                public Request<StringWrapper, MultiRefByteArrayWrapper>? ReadRequest;
                public Request<Pair<StringWrapper, MultiRefByteArrayWrapper>, BoolWrapper>? WriteRequest;
                public Request<StringWrapper, BoolWrapper>? EraseRequest;
                
                public void OK_IsExistsRequest(BoolWrapper res)
                {
                    IsExistsRequest!.Value.Response(res);
                    IsExistsRequest = null;
                }
                
                public void OK_GetSubPathsRequest(StructsArray<StringWrapper> res)
                {
                    GetSubPathsRequest!.Value.Response(res);
                    GetSubPathsRequest = null;
                }
                
                public void OK_ReadRequest(IMultiRefByteArray? res)
                {
                    if (res == null)
                    {
                        ReadRequest!.Value.Fail("Failed to read file");
                    }
                    else
                    {
                        ReadRequest!.Value.Response(new MultiRefByteArrayWrapper() { Value = res });
                        res.Release();
                    }
                    ReadRequest = null;
                }
                
                public void OK_WriteRequest(BoolWrapper res)
                {
                    WriteRequest!.Value.Response(res);
                    WriteRequest = null;
                }

                public void OK_EraseRequest(BoolWrapper res)
                {
                    EraseRequest!.Value.Response(res);
                    EraseRequest = null;
                }
                
                public void Fail_IsExistsRequest(Exception ex)
                {
                    IsExistsRequest!.Value.Fail(ex.Message);
                    IsExistsRequest = null;
                }
                
                public void Fail_GetSubPathsRequest(Exception ex)
                {
                    GetSubPathsRequest!.Value.Fail(ex.Message);
                    GetSubPathsRequest = null;
                }
                
                public void Fail_ReadRequest(Exception ex)
                {
                    ReadRequest!.Value.Fail(ex.Message);
                    ReadRequest = null;
                }
                
                public void Fail_WriteRequest(Exception ex)
                {
                    WriteRequest!.Value.Fail(ex.Message);
                    WriteRequest = null;
                }

                public void Fail_EraseRequest(Exception ex)
                {
                    EraseRequest!.Value.Fail(ex.Message);
                    EraseRequest = null;
                }
            }
        }
    }
}