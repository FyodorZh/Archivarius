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
        
        public event Action<RemoteStorageCommandInfo>? OnCommand;

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
                    res.OnCommand += OnCommand;
                    return res;
                }));
            return transport.Start(_ => { });
        }

        private class ServerSideStorageApi : ServerSideApi<RemoteStorageApi>
        {
            private readonly StorageBackendLogic<UserData> _storage;
            
            public event Action<RemoteStorageCommandInfo>? OnCommand;
            
            public ServerSideStorageApi(IReadOnlySyncStorageBackend storage, IMemoryRental memoryRental, ILogger logger) 
                : base(new RemoteStorageApi(), memoryRental, logger)
            {
                if (storage is ISyncStorageBackend wStorage)
                {
                    _storage = new StorageBackendLogic<UserData>(wStorage, memoryRental);
                }
                else
                {
                    _storage = new StorageBackendLogic<UserData>(storage, memoryRental);
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

            private void IsExists(IRequest<StringWrapper, BoolWrapper> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("IsExists");
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData(this);
                userData.IsExistsRequest = request;
                userData.DbgInfo = info;
                _storage.IsExists(filePath, userData, 
                    (res, data) => data.OK_IsExistsRequest(new BoolWrapper(res)),
                    (e, data) => data.Fail_IsExistsRequest(e));
            }

            private void GetSubPath(IRequest<StringWrapper, StructsArray<StringWrapper>> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("GetSubPaths");
                DirPath dirPath;
                try
                {
                    dirPath = PathFactory.BuildDir(request.Data.Value ?? throw new Exception("DirPath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }

                var userData = _storage.GetFreeUserData() ?? new UserData(this);
                userData.GetSubPathsRequest = request;
                userData.DbgInfo = info;
                _storage.GetSubPath(dirPath, userData, 
                    (res, data) =>
                    {
                        var model = new StructsArray<StringWrapper>(res.Select(fPath => new StringWrapper(fPath.FullName)).ToArray());
                        data.OK_GetSubPathsRequest(model);
                    },
                    (e, data) => data.Fail_GetSubPathsRequest(e));
            }

            private void Read(IRequest<StringWrapper, MultiRefByteArrayWrapper> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Read");
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }

                var userData = _storage.GetFreeUserData() ?? new UserData(this);
                userData.ReadRequest = request;
                userData.DbgInfo = info;
                _storage.Read(filePath, userData, 
                    (res, data) => data.OK_ReadRequest(res),
                    (e, data) => data.Fail_ReadRequest(e));
            }

            private void Write(IRequest<Pair<StringWrapper, MultiRefByteArrayWrapper>, BoolWrapper> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Write");
                
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.First.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }

                if (request.Data.Second.Value == null)
                {
                    request.Fail("Nothing to write");
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData(this);
                userData.WriteRequest = request;
                userData.DbgInfo = info;
                _storage.Write(filePath, request.Data.Second.Value, userData, 
                    (res, data) => data.OK_WriteRequest(new BoolWrapper(res)), 
                    (e, data) => data.Fail_WriteRequest(e));
            }

            private void Delete(IRequest<StringWrapper, BoolWrapper> request)
            {
                RemoteStorageCommandInfo info = new RemoteStorageCommandInfo("Delete");
                
                FilePath filePath;
                try
                {
                    filePath = PathFactory.BuildFile(request.Data.Value ?? throw new Exception("FilePath is null"));
                }
                catch (Exception ex)
                {
                    request.Fail(ex.Message);
                    info.Finish(false);
                    OnCommand?.Invoke(info);
                    return;
                }
                
                var userData = _storage.GetFreeUserData() ?? new UserData(this);
                userData.EraseRequest = request;
                userData.DbgInfo = info;
                _storage.Erase(filePath, userData, 
                    (res, data) => data.OK_EraseRequest(new BoolWrapper(res)), 
                    (e, data) => data.Fail_EraseRequest(e));
            }
            
            private void InvokeOnCommand(RemoteStorageCommandInfo info)
            {
                if (OnCommand != null)
                {
                    OnCommand(info);
                }
            }

            private class UserData
            {
                public readonly ServerSideStorageApi Owner;
                public IRequest<StringWrapper, BoolWrapper>? IsExistsRequest;
                public IRequest<StringWrapper, StructsArray<StringWrapper>>? GetSubPathsRequest;
                public IRequest<StringWrapper, MultiRefByteArrayWrapper>? ReadRequest;
                public IRequest<Pair<StringWrapper, MultiRefByteArrayWrapper>, BoolWrapper>? WriteRequest;
                public IRequest<StringWrapper, BoolWrapper>? EraseRequest;
                public RemoteStorageCommandInfo DbgInfo;
                
                public UserData(ServerSideStorageApi owner)
                {
                    Owner = owner;
                }
                
                public void OK_IsExistsRequest(BoolWrapper res)
                {
                    IsExistsRequest!.Response(res);
                    DbgInfo.Finish(true);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void OK_GetSubPathsRequest(StructsArray<StringWrapper> res)
                {
                    GetSubPathsRequest!.Response(res);
                    DbgInfo.Finish(true);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void OK_ReadRequest(IMultiRefByteArray? res)
                {
                    if (res == null)
                    {
                        ReadRequest!.Fail("Failed to read file");
                        DbgInfo.Finish(false);
                    }
                    else
                    {
                        ReadRequest!.Response(new MultiRefByteArrayWrapper() { Value = res });
                        res.Release();
                        DbgInfo.Finish(true);
                    }
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void OK_WriteRequest(BoolWrapper res)
                {
                    WriteRequest!.Response(res);
                    DbgInfo.Finish(true);
                    Owner.InvokeOnCommand(DbgInfo);
                }

                public void OK_EraseRequest(BoolWrapper res)
                {
                    EraseRequest!.Response(res);
                    DbgInfo.Finish(true);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void Fail_IsExistsRequest(Exception ex)
                {
                    IsExistsRequest!.Fail(ex.Message);
                    DbgInfo.Finish(false);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void Fail_GetSubPathsRequest(Exception ex)
                {
                    GetSubPathsRequest!.Fail(ex.Message);
                    DbgInfo.Finish(false);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void Fail_ReadRequest(Exception ex)
                {
                    ReadRequest!.Fail(ex.Message);
                    DbgInfo.Finish(false);
                    Owner.InvokeOnCommand(DbgInfo);
                }
                
                public void Fail_WriteRequest(Exception ex)
                {
                    WriteRequest!.Fail(ex.Message);
                    DbgInfo.Finish(false);
                    Owner.InvokeOnCommand(DbgInfo);
                }

                public void Fail_EraseRequest(Exception ex)
                {
                    EraseRequest!.Fail(ex.Message);
                    DbgInfo.Finish(false);
                    Owner.InvokeOnCommand(DbgInfo);
                }
            }
        }
    }
}