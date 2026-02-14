using System;
using System.IO;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using Archivarius.Internals;
using BinaryReader = Archivarius.BinaryBackend.BinaryReader;

namespace Archivarius
{
    public interface IMultiDeserializer
    {
        bool DeserializeClass<TData>(byte[] bytes, out TData? data) where TData : class, IDataStruct;
        bool DeserializeClass<TData>(Stream stream, out TData? data) where TData : class, IDataStruct;
        Task<(bool, TData?)> DeserializeClass<TData>(byte[] bytes) where TData : class, IDataStruct;
        Task<(bool, TData?)> DeserializeClass<TData>(Stream stream) where TData : class, IDataStruct;

        bool DeserializeStruct<TData>(byte[] bytes, out TData data) where TData : struct, IDataStruct;
        bool DeserializeStruct<TData>(Stream stream, out TData data) where TData : struct, IDataStruct;
        Task<(bool, TData)> DeserializeStruct<TData>(Stream stream) where TData : struct, IDataStruct;
        Task<(bool, TData)> DeserializeStruct<TData>(byte[] bytes) where TData : struct, IDataStruct;
        
        bool DeserializeVersionedStruct<TData>(byte[] bytes, out TData data) where TData : struct, IVersionedDataStruct;
        bool DeserializeVersionedStruct<TData>(Stream stream, out TData data) where TData : struct, IVersionedDataStruct;
        Task<(bool, TData)> DeserializeVersionedStruct<TData>(byte[] bytes) where TData : struct, IVersionedDataStruct;
        Task<(bool, TData)> DeserializeVersionedStruct<TData>(Stream stream) where TData : struct, IVersionedDataStruct;
    }
    
    public class MultiDeserializer :IMultiDeserializer
    {
        private readonly ObjectPool<DeserializationInstance> _byteDeserializers;
        private readonly ObjectPool<DeserializationStreamInstance> _streamDeserializers;
        
        public MultiDeserializer(Func<ITypeDeserializer> typeDeserializerFactory)
        {
            _byteDeserializers = new ObjectPool<DeserializationInstance>(
                () => new DeserializationInstance(typeDeserializerFactory.Invoke()),
                instance => instance.Reset());
            _streamDeserializers = new ObjectPool<DeserializationStreamInstance>(
                () => new DeserializationStreamInstance(typeDeserializerFactory.Invoke()), 
                instance => instance.Reset());
        }
        
        public MultiDeserializer()
        {
            _byteDeserializers = new ObjectPool<DeserializationInstance>(
                () => new DeserializationInstance(),
                instance => instance.Reset());
            _streamDeserializers = new ObjectPool<DeserializationStreamInstance>(
                () => new DeserializationStreamInstance(), 
                instance => instance.Reset());
        }

        public bool DeserializeClass<TData>(byte[] bytes, out TData? data) where TData : class, IDataStruct
        {
            var deserializer = _byteDeserializers.Get();
            try
            {
                return deserializer.DeserializeClass(bytes, out data);
            }
            finally
            {
                _byteDeserializers.Release(deserializer);
            }
        }

        public bool DeserializeClass<TData>(Stream stream, out TData? data) where TData : class, IDataStruct
        {
            var deserializer = _streamDeserializers.Get();
            try
            {
                return deserializer.DeserializeClass(stream, out data);
            }
            finally
            {
                _streamDeserializers.Release(deserializer);
            }
        }

        public async Task<(bool, TData?)> DeserializeClass<TData>(byte[] bytes)
            where TData : class, IDataStruct
        {
            var deserializer = await _byteDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeClass<TData>(bytes, out var data), data);
            }
            finally
            {
                await _byteDeserializers.ReleaseAsync(deserializer);
            }
        }
        
        public async Task<(bool, TData?)> DeserializeClass<TData>(Stream stream)
            where TData : class, IDataStruct
        {
            var deserializer = await _streamDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeClass<TData>(stream, out var data), data);
            }
            finally
            {
                await _streamDeserializers.ReleaseAsync(deserializer);
            }
        }

        public bool DeserializeStruct<TData>(byte[] bytes, out TData data) where TData : struct, IDataStruct
        {
            var deserializer = _byteDeserializers.Get();
            try
            {
                return deserializer.DeserializeStruct(bytes, out data);
            }
            finally
            {
                _byteDeserializers.Release(deserializer);
            }
        }

        public bool DeserializeStruct<TData>(Stream stream, out TData data) where TData : struct, IDataStruct
        {
            var deserializer = _streamDeserializers.Get();
            try
            {
                return deserializer.DeserializeStruct(stream, out data);
            }
            finally
            {
                _streamDeserializers.Release(deserializer);
            }
        }

        public async Task<(bool, TData)> DeserializeStruct<TData>(byte[] bytes)
            where TData : struct, IDataStruct
        {
            var deserializer = await _byteDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeStruct<TData>(bytes, out var data), data);
            }
            finally
            {
                await _byteDeserializers.ReleaseAsync(deserializer);
            }
        }
        
        public async Task<(bool, TData)> DeserializeStruct<TData>(Stream stream)
            where TData : struct, IDataStruct
        {
            var deserializer = await _streamDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeStruct<TData>(stream, out var data), data);
            }
            finally
            {
                await _streamDeserializers.ReleaseAsync(deserializer);
            }
        }

        public bool DeserializeVersionedStruct<TData>(byte[] bytes, out TData data) where TData : struct, IVersionedDataStruct
        {
            var deserializer = _byteDeserializers.Get();
            try
            {
                return deserializer.DeserializeVersionedStruct(bytes, out data);
            }
            finally
            {
                _byteDeserializers.Release(deserializer);
            }
        }

        public bool DeserializeVersionedStruct<TData>(Stream stream, out TData data) where TData : struct, IVersionedDataStruct
        {
            var deserializer = _streamDeserializers.Get();
            try
            {
                return deserializer.DeserializeVersionedStruct(stream, out data);
            }
            finally
            {
                _streamDeserializers.Release(deserializer);
            }
        }

        public async Task<(bool, TData)> DeserializeVersionedStruct<TData>(byte[] bytes)
            where TData : struct, IVersionedDataStruct
        {
            var deserializer = await _byteDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeVersionedStruct<TData>(bytes, out var data), data);
            }
            finally
            {
                await _byteDeserializers.ReleaseAsync(deserializer);
            }
        }
        
        public async Task<(bool, TData)> DeserializeVersionedStruct<TData>(Stream stream)
            where TData : struct, IVersionedDataStruct
        {
            var deserializer = await _streamDeserializers.GetAsync();
            try
            {
                return (deserializer.DeserializeVersionedStruct<TData>(stream, out var data), data);
            }
            finally
            {
                await _streamDeserializers.ReleaseAsync(deserializer);
            }
        }
        
        private class DeserializationInstance
        {
            private readonly BinaryReader _reader;
            private readonly HierarchicalDeserializer _deserializer;
            private bool _deserializationFail;

            public DeserializationInstance(ITypeDeserializer typeDeserializer)
            {
                _reader = new BinaryReader(Array.Empty<byte>());
                _deserializer = HierarchicalDeserializer.From(_reader).SetAutoPrepare(false).SetPolymorphic(typeDeserializer).Build();
                _deserializer.OnException += _ =>
                {
                    _deserializationFail = true;
                };
            }
            
            public DeserializationInstance()
            {
                _reader = new BinaryReader(Array.Empty<byte>());
                _deserializer = HierarchicalDeserializer.From(_reader).SetAutoPrepare(false).SetMonomorphic().Build();
                _deserializer.OnException += _ =>
                {
                    _deserializationFail = true;
                };
            }

            public void Reset()
            {
                _deserializationFail = false;
            }

            public bool DeserializeClass<TData>(byte[] bytes, out TData? data)
                where TData : class, IDataStruct
            {
                data = null!;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddClass(ref data);
                return !_deserializationFail;
            }
            
            public bool DeserializeStruct<TData>(byte[] bytes, out TData data)
                where TData : struct, IDataStruct
            {
                data = default;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddStruct(ref data);
                return !_deserializationFail;
            }
            
            public bool DeserializeVersionedStruct<TData>(byte[] bytes, out TData data)
                where TData : struct, IVersionedDataStruct
            {
                data = default;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddVersionedStruct(ref data);
                return !_deserializationFail;
            }
        }
        
        private class DeserializationStreamInstance
        {
            private readonly BinaryStreamReader _reader;
            private readonly HierarchicalDeserializer _deserializer;
            private bool _deserializationFail;

            public DeserializationStreamInstance(ITypeDeserializer typeDeserializer)
            {
                _reader = new BinaryStreamReader(new MemoryStream());
                _deserializer = HierarchicalDeserializer.From(_reader).SetAutoPrepare(false).SetPolymorphic(typeDeserializer).Build();
                _deserializer.OnException += _ =>
                {
                    _deserializationFail = true;
                };
            }
            
            public DeserializationStreamInstance()
            {
                _reader = new BinaryStreamReader(new MemoryStream());
                _deserializer = HierarchicalDeserializer.From(_reader).SetAutoPrepare(false).SetMonomorphic().Build();
                _deserializer.OnException += _ =>
                {
                    _deserializationFail = true;
                };
            }

            public void Reset()
            {
                _deserializationFail = false;
            }

            public bool DeserializeClass<TData>(Stream bytes, out TData? data)
                where TData : class, IDataStruct
            {
                data = null!;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddClass(ref data);
                return !_deserializationFail;
            }
            
            public bool DeserializeStruct<TData>(Stream bytes, out TData data)
                where TData : struct, IDataStruct
            {
                data = default;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddStruct(ref data);
                return !_deserializationFail;
            }
            
            public bool DeserializeVersionedStruct<TData>(Stream bytes, out TData data)
                where TData : struct, IVersionedDataStruct
            {
                data = default;
                _reader.Reset(bytes);
                _deserializer.Prepare();
                _deserializer.AddVersionedStruct(ref data);
                return !_deserializationFail;
            }
        }
    }
}