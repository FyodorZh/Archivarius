using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class KeyValueStorage : IKeyValueStorage
    {
        private enum StorageType : byte
        {
            Class = 0,
            Struct = 1,
            VersionedStruct = 2
        }
        
        private readonly IStorageBackend _storage;

        private readonly MultiSerializer _serializer;
        private readonly MultiDeserializer _deserializer;

        public KeyValueStorage(IStorageBackend storage, ITypeSerializer typeSerializer, ITypeDeserializer typeDeserializer)
        {
            _storage = storage;

            _serializer = new MultiSerializer(() => typeSerializer);
            _deserializer = new MultiDeserializer(() => typeDeserializer);
        }

        public async Task Set<TData>(FilePath path, TData data) where TData : class, IDataStruct
        {
            var bytes = await _serializer.SerializeClassAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return default;
            });
        }

        public async Task SetStruct<TData>(FilePath path, TData data) where TData : struct, IDataStruct
        {
            var bytes = await _serializer.SerializeStructAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return default;
            });
        }

        public async Task SetVersionedStruct<TData>(FilePath path, TData data) where TData : struct, IVersionedDataStruct
        {
            var bytes = await _serializer.SerializeVersionedStructAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return default;
            });
        }

        public async Task<TData?> Get<TData>(FilePath path) where TData : class, IDataStruct
        {
            TData? result = null;
            await _storage.Read(path, async stream =>
            {
                var (success, data) = await _deserializer.DeserializeClass<TData>(stream);
                if (!success)
                {
                    throw new Exception("Failed to deserialize data");
                }
                result = data;
            });
            return result;
        }

        public async Task<TData?> GetStruct<TData>(FilePath path) where TData : struct, IDataStruct
        {
            TData? result = null;
            await _storage.Read(path, async stream =>
            {
                var (success, data) = await _deserializer.DeserializeStruct<TData>(stream);
                if (!success)
                {
                    throw new Exception("Failed to deserialize data");
                }
                result = data;
            });
            return result;
        }
        
        public async Task<TData?> GetVersionedStruct<TData>(FilePath path) where TData : struct, IVersionedDataStruct
        {
            TData? result = null;
            await _storage.Read(path, async stream =>
            {
                var (success, data) = await _deserializer.DeserializeVersionedStruct<TData>(stream);
                if (!success)
                {
                    throw new Exception("Failed to deserialize data");
                }
                result = data;
            });
            return result;
        }

        public Task Erase(FilePath path)
        {
            return _storage.Erase(path);
        }

        public Task<bool> IsExists(FilePath path)
        {
            return _storage.IsExists(path);
        }

        public Task<IReadOnlyCollection<FilePath>> GetElements(DirPath path)
        {
            return _storage.GetSubPaths(path);
        }
    }
}