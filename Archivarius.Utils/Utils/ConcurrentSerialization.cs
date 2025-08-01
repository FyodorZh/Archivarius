using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using BinaryWriter = Archivarius.BinaryBackend.BinaryWriter;

namespace Archivarius
{
    public class ConcurrentSerialization
    {
        private readonly Func<ITypeSerializer> _typeSerializerFactory;
        private readonly Func<ITypeDeserializer> _typeDeserializerFactory;

        private readonly Stack<SerializationInstance> _serializers = new();
        private readonly Stack<DeserializationInstance> _deserializers = new();
        
        private readonly SemaphoreSlim _locker = new(1, 1);
        
        public ConcurrentSerialization(
            Func<ITypeSerializer> typeSerializerFactory,
            Func<ITypeDeserializer> typeDeserializerFactory)
        {
            _typeSerializerFactory = typeSerializerFactory;
            _typeDeserializerFactory = typeDeserializerFactory;
        }

        private async Task<SerializationInstance> AcquireSerializer()
        {
            try
            {
                await _locker.WaitAsync();
                if (_serializers.Count > 0)
                {
                    return _serializers.Pop();
                }

                return new SerializationInstance(_typeSerializerFactory.Invoke());
            }
            finally
            {
                _locker.Release();
            }
        }

        private async Task ReleaseSerializer(SerializationInstance serializer)
        {
            try
            {
                serializer.Reset();
                await _locker.WaitAsync();
                _serializers.Push(serializer);
            }
            finally
            {
                _locker.Release();
            }
        }
        
        private async Task<DeserializationInstance> AcquireDeserializer()
        {
            try
            {
                await _locker.WaitAsync();
                if (_deserializers.Count > 0)
                {
                    return _deserializers.Pop();
                }

                return new DeserializationInstance(_typeDeserializerFactory.Invoke());
            }
            finally
            {
                _locker.Release();
            }
        }

        private async Task ReleaseDeserializer(DeserializationInstance deserializer)
        {
            try
            {
                deserializer.Reset();
                await _locker.WaitAsync();
                _deserializers.Push(deserializer);
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<byte[]> SerializeClass<TData>(TData data)
            where TData : class, IDataStruct
        {
            var serializer = await AcquireSerializer();
            try
            {
                return serializer.SerializeClass(data);
            }
            finally
            {
                await ReleaseSerializer(serializer);
            }
        }
        
        public async Task<byte[]> SerializeStruct<TData>(TData data)
            where TData : struct, IDataStruct
        {
            var serializer = await AcquireSerializer();
            try
            {
                return serializer.SerializeStruct(data);
            }
            finally
            {
                await ReleaseSerializer(serializer);
            }
        }
        
        public async Task<byte[]> SerializeVersionedStruct<TData>(TData data)
            where TData : struct, IVersionedDataStruct
        {
            var serializer = await AcquireSerializer();
            try
            {
                return serializer.SerializeVersionedStruct(data);
            }
            finally
            {
                await ReleaseSerializer(serializer);
            }
        }
        
        
        public async Task<(bool, TData?)> DeserializeClass<TData>(byte[] bytes)
            where TData : class, IDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeClass<TData>(new MemoryStream(bytes), out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        
        public async Task<(bool, TData?)> DeserializeClass<TData>(Stream stream)
            where TData : class, IDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeClass<TData>(stream, out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        
        
        public async Task<(bool, TData)> DeserializeStruct<TData>(byte[] bytes)
            where TData : struct, IDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeStruct<TData>(new MemoryStream(bytes), out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        
        public async Task<(bool, TData)> DeserializeStruct<TData>(Stream stream)
            where TData : struct, IDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeStruct<TData>(stream, out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        
        
        public async Task<(bool, TData)> DeserializeVersionedStruct<TData>(byte[] bytes)
            where TData : struct, IVersionedDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeVersionedStruct<TData>(new MemoryStream(bytes), out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        
        public async Task<(bool, TData)> DeserializeVersionedStruct<TData>(Stream stream)
            where TData : struct, IVersionedDataStruct
        {
            var deserializer = await AcquireDeserializer();
            try
            {
                if (deserializer.DeserializeVersionedStruct<TData>(stream, out var data))
                {
                    return (true, data);
                }

                return (false, data);
            }
            finally
            {
                await ReleaseDeserializer(deserializer);
            }
        }
        

        private class SerializationInstance
        {
            private readonly BinaryWriter _writer;
            private readonly HierarchicalSerializer _serializer;

            public SerializationInstance(ITypeSerializer typeSerializer)
            {
                _writer = new BinaryWriter();
                _serializer = new HierarchicalSerializer(_writer, typeSerializer);
            }

            public void Reset()
            {
                _writer.Clear();
                _serializer.Prepare();
            }

            public byte[] SerializeClass<TData>(TData? data)
                where TData : class, IDataStruct
            {
                _serializer.AddClass(ref data);
                return _writer.GetBuffer();
            }
            
            public byte[] SerializeStruct<TData>(TData data)
                where TData : struct, IDataStruct
            {
                _serializer.AddStruct(ref data);
                return _writer.GetBuffer();
            }
            
            public byte[] SerializeVersionedStruct<TData>(TData data)
                where TData : struct, IVersionedDataStruct
            {
                _serializer.AddVersionedStruct(ref data);
                return _writer.GetBuffer();
            }
        }
        
        private class DeserializationInstance
        {
            private readonly BinaryStreamReader _reader;
            private readonly HierarchicalDeserializer _deserializer;
            private bool _deserializationFail;

            public DeserializationInstance(ITypeDeserializer typeDeserializer)
            {
                _reader = new BinaryStreamReader(new MemoryStream());
                _deserializer = new HierarchicalDeserializer(_reader, typeDeserializer, null, null, false);
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