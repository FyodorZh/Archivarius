using System.Collections.Generic;
using System.IO;
using Archivarius.BinaryBackend;
using BinaryWriter = Archivarius.BinaryBackend.BinaryWriter;

namespace Archivarius.Storage
{
    public class SeriaStorage
    {
        private readonly IStorageBackend _storage;

        private readonly BinaryWriter _writer;
        private readonly BinaryStreamReader _reader;

        private readonly HierarchicalSerializer _serializer;
        private readonly HierarchicalDeserializer _deserializer;
        //private bool _deserializationFail;

        //private static readonly string Mark = "@@map";
        
        public SeriaStorage(IStorageBackend storage, ITypeSerializer typeSerializer, ITypeDeserializer typeDeserializer)
        {
            _storage = storage;

            _writer = new BinaryWriter();
            _serializer = new HierarchicalSerializer(_writer, typeSerializer);
            
            _reader = new BinaryStreamReader(new MemoryStream(_writer.GetBuffer()));
            _deserializer = new HierarchicalDeserializer(_reader, typeDeserializer);
            _deserializer.OnException += _ =>
            {
               // _deserializationFail = true;
            };
        }
        /*
        public void AppendSeria<TData>(string path, TData data)
            where TData : struct, IDataStruct
        {
            lock (_serializer)
            {
                _writer.Clear();
                _serializer.Prepare();
                _writer.GetBufferUnsafe(out int headerSize);
                _serializer.AddStruct(ref data);
                var bytes = _writer.GetBufferUnsafe(out int size);
                _storage.Append(path, bytes, headerSize, size - headerSize);
            }
        }
        
        public IReadOnlyList<TData>? GetStream<TData>(string path)
            where TData : struct, IDataStruct
        {
            using var stream = _storage.Read(path);
            if (stream != null)
            {
                lock (_deserializer)
                {
                    _reader.Reset(stream);
                    _deserializer.Prepare();
                    _deserializationFail = false;

                    List<TData> list = new();
                    while (stream.Position < stream.Length)
                    {
                        TData data = default;
                        _deserializer.AddStruct(ref data);
                        if (_deserializationFail)
                        {
                            return null;
                        }
                        list.Add(data);
                    }

                    return list;
                }
            }

            return null;
        }
        */
        public void Erase(string path)
        {
            //_storage.Erase(path + Mark);
            //_storage.Erase(path);
        }
    }
}