using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class ReadOnlyKeyValueStorage : IReadOnlyKeyValueStorage
    {
        private readonly IReadOnlyStorageBackend _storage;
        protected readonly MultiDeserializer _deserializer;

        public ReadOnlyKeyValueStorage(IReadOnlyStorageBackend storage, ITypeDeserializer typeDeserializer)
        {
            _storage = storage;
            _deserializer = new MultiDeserializer(() => typeDeserializer);
        }

        protected ReadOnlyKeyValueStorage(IReadOnlyStorageBackend storage, MultiDeserializer deserializer)
        {
            _storage = storage;
            _deserializer = deserializer;
        }
        
        IReadOnlyKeyValueStorage IReadOnlyKeyValueStorage.SubDirectory(DirPath path)
        {
            return new ReadOnlyKeyValueStorage(_storage.SubDirectory(path), _deserializer);
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

        public Task<bool> IsExists(FilePath path)
        {
            return _storage.IsExists(path);
        }

        public Task<IReadOnlyCollection<FilePath>> GetElements(DirPath path)
        {
            return _storage.GetSubPaths(path);
        }
    }
    
    public class KeyValueStorage : ReadOnlyKeyValueStorage, IKeyValueStorage
    {
        private readonly IStorageBackend _storage;

        private readonly MultiSerializer _serializer;

        public KeyValueStorage(IStorageBackend storage, ITypeSerializer typeSerializer, ITypeDeserializer typeDeserializer)
            : this(storage, new MultiSerializer(() => typeSerializer), new MultiDeserializer(() => typeDeserializer))
        {
        }

        public KeyValueStorage(IStorageBackend storage, MultiSerializer serializer, MultiDeserializer deserializer)
            : base(storage, deserializer)
        {
            _storage = storage;
            _serializer = serializer;
        }
        
        public IKeyValueStorage SubDirectory(DirPath path)
        {
            return new KeyValueStorage(_storage.SubDirectory(path), _serializer, _deserializer);
        }

        public async Task Set<TData>(FilePath path, TData data) where TData : class, IDataStruct
        {
            var bytes = await _serializer.SerializeClassAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            });
        }

        public async Task SetStruct<TData>(FilePath path, TData data) where TData : struct, IDataStruct
        {
            var bytes = await _serializer.SerializeStructAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            });
        }

        public async Task SetVersionedStruct<TData>(FilePath path, TData data) where TData : struct, IVersionedDataStruct
        {
            var bytes = await _serializer.SerializeVersionedStructAsync(data);
            await _storage.Write(path, dst =>
            {
                dst.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            });
        }

        public Task Erase(FilePath path)
        {
            return _storage.Erase(path);
        }
    }
}