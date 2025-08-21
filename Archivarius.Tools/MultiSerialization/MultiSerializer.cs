using System;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using Archivarius.Internals;
using BinaryWriter = Archivarius.BinaryBackend.BinaryWriter;

namespace Archivarius
{
    public interface IMultiSerializer
    {
        byte[] SerializeClass<TData>(TData data) where TData : class, IDataStruct;
        Task<byte[]> SerializeClassAsync<TData>(TData data) where TData : class, IDataStruct;

        byte[] SerializeStruct<TData>(TData data) where TData : struct, IDataStruct;
        Task<byte[]> SerializeStructAsync<TData>(TData data) where TData : struct, IDataStruct;
        
        byte[] SerializeVersionedStruct<TData>(TData data) where TData : struct, IVersionedDataStruct;
        Task<byte[]> SerializeVersionedStructAsync<TData>(TData data) where TData : struct, IVersionedDataStruct;
    }
    
    public class MultiSerializer :IMultiSerializer
    {
        private readonly ObjectPool<SerializationInstance> _byteSerializers;
        
        public MultiSerializer(Func<ITypeSerializer> typeSerializerFactory)
        {
            _byteSerializers = new ObjectPool<SerializationInstance>(
                () => new SerializationInstance(typeSerializerFactory.Invoke()),
                instance => instance.Reset());
        }

        public byte[] SerializeClass<TData>(TData data) where TData : class, IDataStruct
        {
            var Serializer = _byteSerializers.Get();
            try
            {
                return Serializer.SerializeClass(data);
            }
            finally
            {
                _byteSerializers.Release(Serializer);
            }
        }

        public async Task<byte[]> SerializeClassAsync<TData>(TData data)
            where TData : class, IDataStruct
        {
            var serializer = await _byteSerializers.GetAsync();
            try
            {
                return serializer.SerializeClass(data);
            }
            finally
            {
                await _byteSerializers.ReleaseAsync(serializer);
            }
        }

        public byte[] SerializeStruct<TData>(TData data) where TData : struct, IDataStruct
        {
            var serializer = _byteSerializers.Get();
            try
            {
                return serializer.SerializeStruct(data);
            }
            finally
            {
                _byteSerializers.Release(serializer);
            }
        }

        public async Task<byte[]> SerializeStructAsync<TData>(TData data)
            where TData : struct, IDataStruct
        {
            var serializer = await _byteSerializers.GetAsync();
            try
            {
                return serializer.SerializeStruct(data);
            }
            finally
            {
                await _byteSerializers.ReleaseAsync(serializer);
            }
        }

        public byte[] SerializeVersionedStruct<TData>(TData data) where TData : struct, IVersionedDataStruct
        {
            var serializer = _byteSerializers.Get();
            try
            {
                return serializer.SerializeVersionedStruct(data);
            }
            finally
            {
                _byteSerializers.Release(serializer);
            }
        }

        public async Task<byte[]> SerializeVersionedStructAsync<TData>(TData data)
            where TData : struct, IVersionedDataStruct
        {
            var serializer = await _byteSerializers.GetAsync();
            try
            {
                return serializer.SerializeVersionedStruct(data);
            }
            finally
            {
                await _byteSerializers.ReleaseAsync(serializer);
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
        /*
        private class SerializationStreamInstance
        {
            private readonly BinaryStreamWriter _writer;
            private readonly HierarchicalSerializer _serializer;

            public SerializationStreamInstance(ITypeSerializer typeSerializer)
            {
                _writer = new BinaryStreamWriter();
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
        */
    }
}